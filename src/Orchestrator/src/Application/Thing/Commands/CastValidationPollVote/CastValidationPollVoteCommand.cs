using Microsoft.Extensions.Logging;

using GoThataway;
using FluentValidation;

using Domain.Results;
using Domain.Errors;
using Domain.Aggregates;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Thing.Commands.CastValidationPollVote;

[RequireAuthorization]
public class CastValidationPollVoteCommand : IRequest<HandleResult<string>>
{
    public required NewThingValidationPollVoteIm Input { get; init; }
    public required string Signature { get; init; }
}

internal class Validator : AbstractValidator<CastValidationPollVoteCommand>
{
    public Validator()
    {
        RuleFor(c => c.Input).SetValidator(new NewThingValidationPollVoteImValidator());
        RuleFor(c => c.Signature).NotEmpty();
    }
}

public class CastValidationPollVoteCommandHandler : IRequestHandler<
    CastValidationPollVoteCommand,
    HandleResult<string>
>
{
    private readonly ILogger<CastValidationPollVoteCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IContractCaller _contractCaller;
    private readonly IFileStorage _fileStorage;
    private readonly IThingRepository _thingRepository;
    private readonly IThingValidationPollVoteRepository _voteRepository;

    public CastValidationPollVoteCommandHandler(
        ILogger<CastValidationPollVoteCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IContractCaller contractCaller,
        IFileStorage fileStorage,
        IThingRepository thingRepository,
        IThingValidationPollVoteRepository voteRepository
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

    public async Task<HandleResult<string>> Handle(CastValidationPollVoteCommand command, CancellationToken ct)
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
                Error = new HandleError($"Not a designated verifier for thing {command.Input.ThingId}")
            };
        }

        var recoveredAddress = _signer.RecoverFromNewThingValidationPollVoteMessage(command.Input, command.Signature);

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

        var orchestratorSignature = _signer.SignNewThingValidationPollVote(
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

        var vote = new ThingValidationPollVote(
            thingId: command.Input.ThingId.Value,
            voterId: _currentPrincipal.Id!,
            voterWalletAddress: _currentPrincipal.WalletAddress!,
            castedAtMs: castedAtUtc.ToUnixTimeMilliseconds(),
            decision: (ThingValidationPollVote.VoteDecision)command.Input.Decision,
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
