using MediatR;
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
    public required NewAssessmentPollVoteIm Input { get; init; }
    public required string Signature { get; init; }
}

internal class Validator : AbstractValidator<CastAssessmentPollVoteCommand>
{
    public Validator()
    {
        RuleFor(c => c.Input).SetValidator(new NewAssessmentPollVoteImValidator());
        RuleFor(c => c.Signature).NotEmpty();
    }
}

internal class CastAssessmentPollVoteCommandHandler : IRequestHandler<CastAssessmentPollVoteCommand, HandleResult<string>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IContractCaller _contractCaller;
    private readonly IFileStorage _fileStorage;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly IAssessmentPollVoteRepository _voteRepository;

    public CastAssessmentPollVoteCommandHandler(
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IContractCaller contractCaller,
        IFileStorage fileStorage,
        ISettlementProposalRepository settlementProposalRepository,
        IAssessmentPollVoteRepository voteRepository
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
                Error = new VoteError(
                    $"Not a designated verifier for proposal {command.Input.SettlementProposalId}"
                )
            };
        }

        var recoveredAddress = _signer.RecoverFromNewAssessmentPollVoteMessage(command.Input, command.Signature);

        var walletAddress = await _contractCaller.GetWalletAddressFor(recoveredAddress);
        if (walletAddress.Substring(2).ToLower() != _currentPrincipal.Id)
        {
            return new()
            {
                Error = new VoteError("Not the owner of the wallet")
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
                Error = new VoteError("Invalid timestamp. Check your system clock")
            };
        }

        var orchestratorSig = _signer.SignNewAssessmentPollVote(
            command.Input,
            walletAddress: walletAddress,
            ownerAddress: recoveredAddress,
            ownerSignature: command.Signature
        );

        var uploadResult = await _fileStorage.UploadJson(new
        {
            Vote = command.Input.ToMessageForSigning(),
            WalletAddress = walletAddress,
            OwnerAddress = recoveredAddress,
            OwnerSignature = command.Signature,
            OrchestratorSignature = orchestratorSig
        });
        if (uploadResult.IsError)
        {
            return new()
            {
                Error = uploadResult.Error
            };
        }

        var vote = new AssessmentPollVote(
            settlementProposalId: command.Input.SettlementProposalId.Value,
            voterId: _currentPrincipal.Id!,
            castedAtMs: castedAtUtc.ToUnixTimeMilliseconds(),
            decision: (AssessmentPollVote.VoteDecision)command.Input.Decision,
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
