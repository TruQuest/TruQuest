using MediatR;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using CastedAcceptancePollVoteEventDm = Domain.Aggregates.Events.CastedAcceptancePollVoteEvent;

namespace Application.Ethereum.Events.AcceptancePoll.CastedAcceptancePollVote;

public class CastedAcceptancePollVoteEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required string UserId { get; init; }
    public required int Vote { get; init; }
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
            thingId: new Guid(@event.ThingId),
            userId: @event.UserId,
            decision: (AcceptancePollVote.VoteDecision)@event.Vote,
            reason: @event.Reason
        );
        _castedAcceptancePollVoteEventRepository.Create(castedVoteEvent);

        await _castedAcceptancePollVoteEventRepository.SaveChanges();
    }
}