using Domain.Base;

namespace Domain.Aggregates.Events;

public class JoinedLotteryEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public string ThingIdHash { get; }
    public string UserId { get; }
    public decimal Nonce { get; }

    public JoinedLotteryEvent(long blockNumber, int txnIndex, string thingIdHash, string userId, decimal nonce)
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingIdHash = thingIdHash;
        UserId = userId;
        Nonce = nonce;
    }
}