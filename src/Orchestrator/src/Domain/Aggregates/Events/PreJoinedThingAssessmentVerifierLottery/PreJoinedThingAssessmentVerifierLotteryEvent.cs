using Domain.Base;

namespace Domain.Aggregates.Events;

public class PreJoinedThingAssessmentVerifierLotteryEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public string ThingIdHash { get; }
    public string SettlementProposalIdHash { get; }
    public string UserId { get; }
    public string DataHash { get; }

    public PreJoinedThingAssessmentVerifierLotteryEvent(
        long blockNumber, int txnIndex, string thingIdHash,
        string settlementProposalIdHash, string userId, string dataHash
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingIdHash = thingIdHash;
        SettlementProposalIdHash = settlementProposalIdHash;
        UserId = userId;
        DataHash = dataHash;
    }
}