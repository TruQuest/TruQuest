using Domain.Base;

namespace Domain.Aggregates.Events;

public class ThingAssessmentVerifierLotterySpotClaimedEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public Guid ThingId { get; }
    public Guid SettlementProposalId { get; }
    public string UserId { get; }

    public ThingAssessmentVerifierLotterySpotClaimedEvent(
        long blockNumber, int txnIndex, Guid thingId,
        Guid settlementProposalId, string userId
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingId = thingId;
        SettlementProposalId = settlementProposalId;
        UserId = userId;
    }
}