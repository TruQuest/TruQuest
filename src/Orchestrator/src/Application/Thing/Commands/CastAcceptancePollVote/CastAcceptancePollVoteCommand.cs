using Microsoft.Extensions.Logging;

using MediatR;
using FluentValidation;

using Domain.Results;
using Domain.Errors;
using Domain.Aggregates;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Thing.Commands.CastAcceptancePollVote;

[RequireAuthorization]
public class CastAcceptancePollVoteCommand : IRequest<HandleResult<string>>
{
    public required NewAcceptancePollVoteIm Input { get; init; }
    public required string Signature { get; init; }
}

internal class Validator : AbstractValidator<CastAcceptancePollVoteCommand>
{
    public Validator()
    {
        RuleFor(c => c.Input).SetValidator(new NewAcceptancePollVoteImValidator());
        RuleFor(c => c.Signature).NotEmpty();
    }
}

internal class CastAcceptancePollVoteCommandHandler : IRequestHandler<
    CastAcceptancePollVoteCommand,
    HandleResult<string>
>
{
    private readonly ILogger<CastAcceptancePollVoteCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IContractCaller _contractCaller;
    private readonly IFileStorage _fileStorage;
    private readonly IThingRepository _thingRepository;
    private readonly IAcceptancePollVoteRepository _voteRepository;

    public CastAcceptancePollVoteCommandHandler(
        ILogger<CastAcceptancePollVoteCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IContractCaller contractCaller,
        IFileStorage fileStorage,
        IThingRepository thingRepository,
        IAcceptancePollVoteRepository voteRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _contractCaller = contractCaller;
        _fileStorage = fileStorage;
        _thingRepository = thingRepository;
        _voteRepository = voteRepository;
    }

    public async Task<HandleResult<string>> Handle(CastAcceptancePollVoteCommand command, CancellationToken ct)
    {
        // @@TODO: Use queryable instead of repo.
        bool isDesignatedVerifier = await _thingRepository.CheckIsDesignatedVerifierFor(
            command.Input.ThingId!.Value,
            _currentPrincipal.Id!
        );

        if (!isDesignatedVerifier)
        {
            return new()
            {
                Error = new VoteError($"Not a designated verifier for thing {command.Input.ThingId}")
            };
        }

        var recoveredAddress = _signer.RecoverFromNewAcceptancePollVoteMessage(command.Input, command.Signature);

        if (_currentPrincipal.SignerAddress != recoveredAddress)
        {
            return new()
            {
                Error = new VoteError("Invalid request")
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

        var orchestratorSignature = _signer.SignNewAcceptancePollVote(
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

        var vote = new AcceptancePollVote(
            thingId: command.Input.ThingId.Value,
            voterId: _currentPrincipal.Id!,
            voterWalletAddress: _currentPrincipal.WalletAddress!,
            castedAtMs: castedAtUtc.ToUnixTimeMilliseconds(),
            decision: (AcceptancePollVote.VoteDecision)command.Input.Decision,
            reason: command.Input.Reason != string.Empty ? command.Input.Reason : null,
            voterSignature: command.Signature, // @@??: Do we actually need to store this in db? It is saved in IPFS already.
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
