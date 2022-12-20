using Domain.Base;

namespace Domain.Aggregates.Events;

public class CastedAcceptancePollVoteEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public Guid ThingId { get; }
    public string UserId { get; }
    public Decision Decision { get; }
    public string? Reason { get; }

    public CastedAcceptancePollVoteEvent(
        long blockNumber, int txnIndex, Guid thingId,
        string userId, Decision decision, string? reason
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingId = thingId;
        UserId = userId;
        Decision = decision;
        Reason = reason;
    }
}