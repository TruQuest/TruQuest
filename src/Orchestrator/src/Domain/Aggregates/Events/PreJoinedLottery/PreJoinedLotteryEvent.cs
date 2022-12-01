using Domain.Base;

namespace Domain.Aggregates.Events;

public class PreJoinedLotteryEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public string ThingIdHash { get; }
    public string UserId { get; }
    public string DataHash { get; }

    public PreJoinedLotteryEvent(long blockNumber, int txnIndex, string thingIdHash, string userId, string dataHash)
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingIdHash = thingIdHash;
        UserId = userId;
        DataHash = dataHash;
    }
}