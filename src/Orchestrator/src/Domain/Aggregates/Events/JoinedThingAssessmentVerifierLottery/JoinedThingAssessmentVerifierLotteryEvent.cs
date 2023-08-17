using Domain.Base;

namespace Domain.Aggregates.Events;

public class JoinedThingAssessmentVerifierLotteryEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public string TxnHash { get; }
    public Guid ThingId { get; }
    public Guid SettlementProposalId { get; }
    public string UserId { get; }
    public long L1BlockNumber { get; }
    public string? UserData { get; }
    public long? Nonce { get; private set; }

    public JoinedThingAssessmentVerifierLotteryEvent(
        long blockNumber, int txnIndex, string txnHash, Guid thingId,
        Guid settlementProposalId, string userId, long l1BlockNumber,
        string? userData = null, long? nonce = null
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        TxnHash = txnHash;
        ThingId = thingId;
        SettlementProposalId = settlementProposalId;
        UserId = userId;
        L1BlockNumber = l1BlockNumber;
        UserData = userData;
        Nonce = nonce;
    }

    public void SetNonce(long nonce)
    {
        Nonce = nonce;
    }
}
