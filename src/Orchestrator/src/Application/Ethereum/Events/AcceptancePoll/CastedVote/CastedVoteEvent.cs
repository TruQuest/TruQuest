using MediatR;

using Domain.Aggregates;
using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.AcceptancePoll.CastedVote;

public class CastedVoteEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required string TxnHash { get; init; }
    public required byte[] ThingId { get; init; }
    public required string UserId { get; init; }
    public required int Vote { get; init; }
    public string? Reason { get; init; }
    public required long L1BlockNumber { get; init; }
}

internal class CastedVoteEventHandler : INotificationHandler<CastedVoteEvent>
{
    private readonly ICastedAcceptancePollVoteEventRepository _castedAcceptancePollVoteEventRepository;

    public CastedVoteEventHandler(
        ICastedAcceptancePollVoteEventRepository castedAcceptancePollVoteEventRepository
    )
    {
        _castedAcceptancePollVoteEventRepository = castedAcceptancePollVoteEventRepository;
    }

    public async Task Handle(CastedVoteEvent @event, CancellationToken ct)
    {
        var castedVoteEvent = new CastedAcceptancePollVoteEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            thingId: new Guid(@event.ThingId),
            userId: @event.UserId,
            decision: (AcceptancePollVote.VoteDecision)@event.Vote,
            reason: @event.Reason,
            l1BlockNumber: @event.L1BlockNumber
        );
        _castedAcceptancePollVoteEventRepository.Create(castedVoteEvent);

        await _castedAcceptancePollVoteEventRepository.SaveChanges();
    }
}
