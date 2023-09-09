using Domain.Base;

namespace Domain.Aggregates.Events;

public class ThingAssessmentVerifierLotteryInitializedEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public string TxnHash { get; }
    public long L1BlockNumber { get; }
    public Guid ThingId { get; }
    public Guid SettlementProposalId { get; }
    public string DataHash { get; }
    public string UserXorDataHash { get; }

    public ThingAssessmentVerifierLotteryInitializedEvent(
        long blockNumber, int txnIndex, string txnHash, long l1BlockNumber,
        Guid thingId, Guid settlementProposalId, string dataHash, string userXorDataHash
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        TxnHash = txnHash;
        L1BlockNumber = l1BlockNumber;
        ThingId = thingId;
        SettlementProposalId = settlementProposalId;
        DataHash = dataHash;
        UserXorDataHash = userXorDataHash;
    }
}
