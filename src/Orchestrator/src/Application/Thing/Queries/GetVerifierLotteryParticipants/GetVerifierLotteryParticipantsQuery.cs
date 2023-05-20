using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Thing.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsQuery : IRequest<HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    public required Guid ThingId { get; init; }
}

internal class GetVerifierLotteryParticipantsQueryHandler :
    IRequestHandler<GetVerifierLotteryParticipantsQuery, HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    private readonly IThingQueryable _thingQueryable;
    private readonly ISigner _signer;

    public GetVerifierLotteryParticipantsQueryHandler(
        IThingQueryable thingQueryable,
        ISigner signer
    )
    {
        _thingQueryable = thingQueryable;
        _signer = signer;
    }

    public async Task<HandleResult<GetVerifierLotteryParticipantsResultVm>> Handle(
        GetVerifierLotteryParticipantsQuery query, CancellationToken ct
    )
    {
        var entries = await _thingQueryable.GetVerifierLotteryParticipants(query.ThingId);
        var firstEntry = entries.First();
        if (_signer.CheckIsOrchestrator(firstEntry.UserId) && firstEntry.Nonce != null)
        {
            // means the lottery is completed
            firstEntry.IsOrchestrator = true;
            var verifiers = await _thingQueryable.GetVerifiers(query.ThingId);
            if (verifiers.Any())
            {
                // means it was completed with success
                foreach (var entry in entries)
                {
                    if (verifiers.Any(v => v.VerifierId == entry.UserId))
                    {
                        entry.IsWinner = true;
                    }
                }
            }
        }
        else
        {
            // means the lottery is still in progress
            foreach (var entry in entries.Where(e => e.Nonce == null))
            {
                if (_signer.CheckIsOrchestrator(entry.UserId))
                {
                    entry.IsOrchestrator = true;
                    break;
                }
            }
        }

        entries = entries
            .OrderBy(e => e.SortKey)
            .ThenByDescending(e => e.JoinedBlockNumber);

        return new()
        {
            Data = new()
            {
                ThingId = query.ThingId,
                Entries = entries
            }
        };
    }
}