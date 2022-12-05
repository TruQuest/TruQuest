using MediatR;

using Domain.Results;
using Domain.Errors;
using Domain.Aggregates;
using VoteDm = Domain.Aggregates.Vote;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Vote.Commands.CastVote;

[RequireAuthorization]
public class CastVoteCommand : IRequest<HandleResult<string>>
{
    public NewVoteIm Input { get; set; }
    public string Signature { get; set; }
}

internal class CastVoteCommandHandler : IRequestHandler<CastVoteCommand, HandleResult<string>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IFileStorage _fileStorage;
    private readonly IThingRepository _thingRepository;
    private readonly IVoteRepository _voteRepository;

    public CastVoteCommandHandler(
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IFileStorage fileStorage,
        IThingRepository thingRepository,
        IVoteRepository voteRepository
    )
    {
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _fileStorage = fileStorage;
        _thingRepository = thingRepository;
        _voteRepository = voteRepository;
    }

    public async Task<HandleResult<string>> Handle(CastVoteCommand command, CancellationToken ct)
    {
        bool isValidVerifier = await _thingRepository.CheckIsVerifierFor(
            command.Input.ThingId,
            _currentPrincipal.Id
        );

        if (!isValidVerifier)
        {
            return new()
            {
                Error = new VoteError($"Not a valid verifier for thing {command.Input.ThingId}")
            };
        }

        var result = _signer.RecoverFromNewVoteMessage(command.Input, command.Signature);
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        // check that result.Data == _currentPrincipal.Id
        // @@??: Check current block ?

        var castedAtUtc = DateTime.Parse(command.Input.CastedAt).ToUniversalTime();
        if ((DateTime.UtcNow - castedAtUtc).Duration() > TimeSpan.FromMinutes(5))
        {
            return new()
            {
                Error = new VoteError("Invalid timestamp. Check your system clock")
            };
        }

        var orchestratorSig = _signer.SignNewVote(command.Input, _currentPrincipal.Id, command.Signature);

        var uploadResult = await _fileStorage.UploadJson(new
        {
            Vote = new
            {
                ThingId = command.Input.ThingId,
                PollType = command.Input.PollType.GetString(),
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

        var vote = new VoteDm(
            thingId: command.Input.ThingId,
            voterId: _currentPrincipal.Id,
            pollType: (PollType)command.Input.PollType,
            castedAtMs: DateTimeOffset.Parse(command.Input.CastedAt).ToUnixTimeMilliseconds(),
            decision: (Decision)command.Input.Decision,
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