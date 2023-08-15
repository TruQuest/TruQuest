using Domain.Base;

namespace Domain.Aggregates.Events;

public class CastedAcceptancePollVoteEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public string TxnHash { get; }
    public Guid ThingId { get; }
    public string UserId { get; }
    public AcceptancePollVote.VoteDecision Decision { get; }
    public string? Reason { get; }
    public long L1BlockNumber { get; }

    public CastedAcceptancePollVoteEvent(
        long blockNumber, int txnIndex, string txnHash, Guid thingId, string userId,
        AcceptancePollVote.VoteDecision decision, string? reason, long l1BlockNumber
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        TxnHash = txnHash;
        ThingId = thingId;
        UserId = userId;
        Decision = decision;
        Reason = reason;
        L1BlockNumber = l1BlockNumber;
    }
}
