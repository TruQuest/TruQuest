using Domain.Base;

namespace Domain.Aggregates.Events;

public class JoinedVerifierLotteryEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public Guid ThingId { get; }
    public string UserId { get; }
    public decimal Nonce { get; }

    public JoinedVerifierLotteryEvent(
        long blockNumber, int txnIndex, Guid thingId, string userId, decimal nonce
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingId = thingId;
        UserId = userId;
        Nonce = nonce;
    }
}