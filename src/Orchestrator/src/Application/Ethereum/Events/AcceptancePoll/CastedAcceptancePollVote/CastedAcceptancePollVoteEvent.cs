using MediatR;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using CastedAcceptancePollVoteEventDm = Domain.Aggregates.Events.CastedAcceptancePollVoteEvent;

namespace Application.Ethereum.Events.AcceptancePoll.CastedAcceptancePollVote;

public class CastedAcceptancePollVoteEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required string ThingIdHash { get; init; }
    public required string UserId { get; init; }
    public int Vote { get; init; }
    public string? Reason { get; init; }
}

internal class CastedAcceptancePollVoteEventHandler : INotificationHandler<CastedAcceptancePollVoteEvent>
{
    private readonly ICastedAcceptancePollVoteEventRepository _castedAcceptancePollVoteEventRepository;

    public CastedAcceptancePollVoteEventHandler(
        ICastedAcceptancePollVoteEventRepository castedAcceptancePollVoteEventRepository
    )
    {
        _castedAcceptancePollVoteEventRepository = castedAcceptancePollVoteEventRepository;
    }

    public async Task Handle(CastedAcceptancePollVoteEvent @event, CancellationToken ct)
    {
        var castedVoteEvent = new CastedAcceptancePollVoteEventDm(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingIdHash: @event.ThingIdHash,
            userId: @event.UserId,
            decision: (Decision)@event.Vote,
            reason: @event.Reason
        );
        _castedAcceptancePollVoteEventRepository.Create(castedVoteEvent);

        await _castedAcceptancePollVoteEventRepository.SaveChanges();
    }
}