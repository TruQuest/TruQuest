using GoThataway;
using FluentValidation;

using Domain.Results;
using Domain.Errors;
using Domain.Aggregates;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Settlement.Commands.CastAssessmentPollVote;

[RequireAuthorization]
public class CastAssessmentPollVoteCommand : IRequest<HandleResult<string>>
{
    public required NewSettlementProposalAssessmentPollVoteIm Input { get; init; }
    public required string Signature { get; init; }
}

internal class Validator : AbstractValidator<CastAssessmentPollVoteCommand>
{
    public Validator()
    {
        RuleFor(c => c.Input).SetValidator(new NewSettlementProposalAssessmentPollVoteImValidator());
        RuleFor(c => c.Signature).NotEmpty();
    }
}

public class CastAssessmentPollVoteCommandHandler : IRequestHandler<CastAssessmentPollVoteCommand, HandleResult<string>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IContractCaller _contractCaller;
    private readonly IFileStorage _fileStorage;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ISettlementProposalAssessmentPollVoteRepository _voteRepository;

    public CastAssessmentPollVoteCommandHandler(
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IContractCaller contractCaller,
        IFileStorage fileStorage,
        ISettlementProposalRepository settlementProposalRepository,
        ISettlementProposalAssessmentPollVoteRepository voteRepository
    )
    {
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _contractCaller = contractCaller;
        _fileStorage = fileStorage;
        _settlementProposalRepository = settlementProposalRepository;
        _voteRepository = voteRepository;
    }

    public async Task<HandleResult<string>> Handle(CastAssessmentPollVoteCommand command, CancellationToken ct)
    {
        // @@TODO: Use queryable instead of repo.
        bool isDesignatedVerifier = await _settlementProposalRepository.CheckIsDesignatedVerifierFor(
            command.Input.SettlementProposalId!.Value,
            _currentPrincipal.Id!
        );

        if (!isDesignatedVerifier)
        {
            return new()
            {
                Error = new HandleError(
                    $"Not a designated verifier for proposal {command.Input.SettlementProposalId}"
                )
            };
        }

        var recoveredAddress = _signer.RecoverFromNewSettlementProposalAssessmentPollVoteMessage(command.Input, command.Signature);

        if (_currentPrincipal.SignerAddress != recoveredAddress)
        {
            return new()
            {
                Error = new HandleError("Invalid request")
            };
        }

        // @@??: Check current block ?

        var castedAtUtc = DateTimeOffset
            .ParseExact(command.Input.CastedAt, "yyyy-MM-dd HH:mm:sszzz", null)
            .ToUniversalTime();
        if ((DateTimeOffset.UtcNow - castedAtUtc).Duration() > TimeSpan.FromMinutes(5)) // @@TODO: Config.
        {
            return new()
            {
                Error = new HandleError("Invalid timestamp. Check your system clock")
            };
        }

        var orchestratorSignature = _signer.SignNewSettlementProposalAssessmentPollVote(
            command.Input,
            userId: _currentPrincipal.Id!,
            walletAddress: _currentPrincipal.WalletAddress!,
            signerAddress: _currentPrincipal.SignerAddress!,
            signature: command.Signature
        );

        var uploadResult = await _fileStorage.UploadJson(new
        {
            Vote = command.Input.ToMessageForSigning(),
            UserId = _currentPrincipal.Id!,
            WalletAddress = _currentPrincipal.WalletAddress!,
            SignerAddress = _currentPrincipal.SignerAddress!,
            Signature = command.Signature,
            OrchestratorSignature = orchestratorSignature
        });
        if (uploadResult.IsError)
        {
            return new()
            {
                Error = uploadResult.Error
            };
        }

        var vote = new SettlementProposalAssessmentPollVote(
            settlementProposalId: command.Input.SettlementProposalId.Value,
            voterId: _currentPrincipal.Id!,
            voterWalletAddress: _currentPrincipal.WalletAddress!,
            castedAtMs: castedAtUtc.ToUnixTimeMilliseconds(),
            decision: (SettlementProposalAssessmentPollVote.VoteDecision)command.Input.Decision,
            reason: command.Input.Reason != string.Empty ? command.Input.Reason : null,
            voterSignature: command.Signature,
            ipfsCid: uploadResult.Data!
        );
        _voteRepository.Create(vote);

        await _voteRepository.SaveChanges();

        return new()
        {
            Data = uploadResult.Data
        };
    }
}
