using Microsoft.Extensions.Logging;

using GoThataway;
using FluentValidation;

using Domain.Results;
using Domain.Errors;
using Domain.Aggregates;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Settlement.Commands.CastAssessmentPollVote;

[RequireAuthorization]
public class CastAssessmentPollVoteCommand : IRequest<HandleResult<string>>
{
    public required NewSettlementProposalAssessmentPollVoteIm Input { get; init; }
    public required string Signature { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(HandleResult<string> response)
    {
        if (response.Error == null)
        {
            return new (string Name, object? Value)[]
            {
                (ActivityTags.SettlementProposalId, Input.SettlementProposalId),
                (ActivityTags.Decision, Input.Decision.GetString()),
                (ActivityTags.IpfsCid, response.Data!)
            };
        }

        return Enumerable.Empty<(string Name, object? Value)>();
    }
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
    private readonly ILogger<CastAssessmentPollVoteCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IContractCaller _contractCaller;
    private readonly IFileStorage _fileStorage;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ISettlementProposalAssessmentPollVoteRepository _voteRepository;

    public CastAssessmentPollVoteCommandHandler(
        ILogger<CastAssessmentPollVoteCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IContractCaller contractCaller,
        IFileStorage fileStorage,
        ISettlementProposalRepository settlementProposalRepository,
        ISettlementProposalAssessmentPollVoteRepository voteRepository
    )
    {
        _logger = logger;
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
            _logger.LogWarning(
                $"User {UserId} trying to cast a vote for settlement proposal {SettlementProposalId}, even though he is not a designated verifier",
                _currentPrincipal.Id!, command.Input.SettlementProposalId.Value
            );
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
            _logger.LogWarning(
                $"Current principal's signer address {SignerAddress} and recovered address {RecoveredAddress} do not match",
                _currentPrincipal.SignerAddress!, recoveredAddress
            );
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
            _logger.LogInformation(
                $"Settlement proposal {SettlementProposalId} assessment poll vote's timestamp does not approximately match server time",
                command.Input.SettlementProposalId.Value
            );
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
            _logger.LogWarning(
                $"Error trying to upload settlement proposal {SettlementProposalId} assessment poll vote to IPFS: {uploadResult.Error}",
                command.Input.SettlementProposalId.Value
            );
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

        _logger.LogInformation(
            $"User {UserId} casted an off-chain settlement proposal {SettlementProposalId} assessment poll vote",
            _currentPrincipal.Id!, command.Input.SettlementProposalId.Value
        );

        return new()
        {
            Data = uploadResult.Data
        };
    }
}
