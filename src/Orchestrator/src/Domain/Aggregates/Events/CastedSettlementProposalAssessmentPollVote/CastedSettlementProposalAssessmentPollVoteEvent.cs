using Domain.Base;

namespace Domain.Aggregates.Events;

public class CastedSettlementProposalAssessmentPollVoteEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public string TxnHash { get; }
    public int LogIndex { get; }
    public Guid ThingId { get; }
    public Guid SettlementProposalId { get; }
    public string? UserId { get; }
    public string WalletAddress { get; }
    public SettlementProposalAssessmentPollVote.VoteDecision Decision { get; }
    public string? Reason { get; }
    public long L1BlockNumber { get; }

    public CastedSettlementProposalAssessmentPollVoteEvent(
        long blockNumber, int txnIndex, string txnHash, int logIndex,
        Guid thingId, Guid settlementProposalId, string walletAddress,
        SettlementProposalAssessmentPollVote.VoteDecision decision,
        string? reason, long l1BlockNumber
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        TxnHash = txnHash;
        LogIndex = logIndex;
        ThingId = thingId;
        SettlementProposalId = settlementProposalId;
        WalletAddress = walletAddress;
        Decision = decision;
        Reason = reason;
        L1BlockNumber = l1BlockNumber;
    }
}
