using Domain.Base;

namespace Domain.Aggregates.Events;

public class CastedAssessmentPollVoteEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public Guid ThingId { get; }
    public Guid SettlementProposalId { get; }
    public string UserId { get; }
    public AssessmentPollVote.VoteDecision Decision { get; }
    public string? Reason { get; }

    public CastedAssessmentPollVoteEvent(
        long blockNumber, int txnIndex, Guid thingId, Guid settlementProposalId,
        string userId, AssessmentPollVote.VoteDecision decision, string? reason
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingId = thingId;
        SettlementProposalId = settlementProposalId;
        UserId = userId;
        Decision = decision;
        Reason = reason;
    }
}