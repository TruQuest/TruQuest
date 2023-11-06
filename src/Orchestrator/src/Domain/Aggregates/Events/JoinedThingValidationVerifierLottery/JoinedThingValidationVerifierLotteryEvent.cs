using Domain.Base;

namespace Domain.Aggregates.Events;

public class JoinedThingValidationVerifierLotteryEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public string TxnHash { get; }
    public int LogIndex { get; }
    public Guid ThingId { get; }
    public string? UserId { get; }
    public string WalletAddress { get; }
    public long L1BlockNumber { get; }
    public string UserData { get; }
    public long? Nonce { get; private set; }

    public JoinedThingValidationVerifierLotteryEvent(
        long blockNumber, int txnIndex, string txnHash, int logIndex,
        Guid thingId, string walletAddress, long l1BlockNumber, string userData
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        TxnHash = txnHash;
        LogIndex = logIndex;
        ThingId = thingId;
        WalletAddress = walletAddress;
        L1BlockNumber = l1BlockNumber;
        UserData = userData;
    }

    public void SetNonce(long nonce)
    {
        Nonce = nonce;
    }
}
