using MediatR;

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

internal class CastAcceptancePollVoteCommandHandler : IRequestHandler<CastAcceptancePollVoteCommand, HandleResult<string>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IFileStorage _fileStorage;
    private readonly IThingRepository _thingRepository;
    private readonly IAcceptancePollVoteRepository _voteRepository;

    public CastAcceptancePollVoteCommandHandler(
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IFileStorage fileStorage,
        IThingRepository thingRepository,
        IAcceptancePollVoteRepository voteRepository
    )
    {
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _fileStorage = fileStorage;
        _thingRepository = thingRepository;
        _voteRepository = voteRepository;
    }

    public async Task<HandleResult<string>> Handle(CastAcceptancePollVoteCommand command, CancellationToken ct)
    {
        // @@TODO: Use queryable instead of repo.
        bool isDesignatedVerifier = await _thingRepository.CheckIsDesignatedVerifierFor(
            command.Input.ThingId,
            _currentPrincipal.Id!
        );

        if (!isDesignatedVerifier)
        {
            return new()
            {
                Error = new VoteError($"Not a designated verifier for thing with id = {command.Input.ThingId}")
            };
        }

        var result = _signer.RecoverFromNewAcceptancePollVoteMessage(command.Input, command.Signature);
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        // check that result.Data == _currentPrincipal.Id
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

        var orchestratorSig = _signer.SignNewAcceptancePollVote(
            command.Input, _currentPrincipal.Id!, command.Signature
        );

        var uploadResult = await _fileStorage.UploadJson(new
        {
            Vote = new
            {
                ThingId = command.Input.ThingId,
                CastedAt = command.Input.CastedAt,
                Decision = command.Input.Decision.GetString(),
                Reason = command.Input.Reason
            },
            VoterId = _currentPrincipal.Id,
            VoterSignature = command.Signature,
            OrchestratorSignature = orchestratorSig
        });
        if (uploadResult.IsError)
        {
            return new()
            {
                Error = uploadResult.Error
            };
        }

        var vote = new AcceptancePollVote(
            thingId: command.Input.ThingId,
            voterId: _currentPrincipal.Id!,
            castedAtMs: castedAtUtc.ToUnixTimeMilliseconds(),
            decision: (AcceptancePollVote.VoteDecision)command.Input.Decision,
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