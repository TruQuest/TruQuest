using Domain.Base;

namespace Domain.Aggregates.Events;

public class JoinedThingAssessmentVerifierLotteryEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public string ThingIdHash { get; }
    public string SettlementProposalIdHash { get; }
    public string UserId { get; }
    public decimal Nonce { get; }

    public JoinedThingAssessmentVerifierLotteryEvent(
        long blockNumber, int txnIndex, string thingIdHash,
        string settlementProposalIdHash, string userId, decimal nonce
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingIdHash = thingIdHash;
        SettlementProposalIdHash = settlementProposalIdHash;
        UserId = userId;
        Nonce = nonce;
    }
}