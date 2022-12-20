using Domain.Base;

namespace Domain.Aggregates.Events;

public class PreJoinedThingAssessmentVerifierLotteryEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public Guid ThingId { get; }
    public Guid SettlementProposalId { get; }
    public string UserId { get; }
    public string DataHash { get; }

    public PreJoinedThingAssessmentVerifierLotteryEvent(
        long blockNumber, int txnIndex, Guid thingId,
        Guid settlementProposalId, string userId, string dataHash
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingId = thingId;
        SettlementProposalId = settlementProposalId;
        UserId = userId;
        DataHash = dataHash;
    }
}