using Domain.Base;

namespace Domain.Aggregates.Events;

public class JoinedThingSubmissionVerifierLotteryEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public Guid ThingId { get; }
    public string UserId { get; }
    public string? UserData { get; }
    public long? Nonce { get; private set; }

    public JoinedThingSubmissionVerifierLotteryEvent(
        long blockNumber, int txnIndex, Guid thingId,
        string userId, string? userData = null, long? nonce = null
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingId = thingId;
        UserId = userId;
        UserData = userData;
        Nonce = nonce;
    }

    public void SetNonce(long nonce)
    {
        Nonce = nonce;
    }
}