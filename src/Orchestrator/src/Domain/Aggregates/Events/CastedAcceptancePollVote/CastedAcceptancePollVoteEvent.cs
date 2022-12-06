using Domain.Base;

namespace Domain.Aggregates.Events;

public class CastedAcceptancePollVoteEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public string ThingIdHash { get; }
    public string UserId { get; }
    public Decision Decision { get; }

    public CastedAcceptancePollVoteEvent(
        long blockNumber, int txnIndex, string thingIdHash, string userId, Decision decision
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingIdHash = thingIdHash;
        UserId = userId;
        Decision = decision;
    }
}