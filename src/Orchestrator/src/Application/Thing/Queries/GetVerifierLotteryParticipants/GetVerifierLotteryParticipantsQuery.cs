using System.Diagnostics;

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
        var firstEntry = entries.FirstOrDefault();
        if (firstEntry != null && _signer.CheckIsOrchestrator(firstEntry.UserId))
        {
            // means the lottery was closed with success
            Debug.Assert(firstEntry.Nonce != null);
            firstEntry.IsOrchestrator = true;

            var verifiers = await _thingQueryable.GetVerifiers(query.ThingId);
            Debug.Assert(verifiers.Any());
            foreach (var verifier in verifiers)
            {
                var entry = entries.Single(e => e.UserId == verifier.VerifierId);
                entry.IsWinner = true;
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