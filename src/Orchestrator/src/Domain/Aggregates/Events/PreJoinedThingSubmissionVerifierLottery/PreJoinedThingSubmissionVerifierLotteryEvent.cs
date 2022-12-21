using Domain.Base;

namespace Domain.Aggregates.Events;

public class PreJoinedThingSubmissionVerifierLotteryEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public Guid ThingId { get; }
    public string UserId { get; }
    public string DataHash { get; }

    public PreJoinedThingSubmissionVerifierLotteryEvent(
        long blockNumber, int txnIndex, Guid thingId, string userId, string dataHash
    )
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingId = thingId;
        UserId = userId;
        DataHash = dataHash;
    }
}