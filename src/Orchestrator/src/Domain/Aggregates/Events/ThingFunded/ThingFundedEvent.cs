using Domain.Base;

namespace Domain.Aggregates.Events;

public class ThingFundedEvent : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public long BlockNumber { get; }
    public string ThingIdHash { get; }
    public string UserId { get; }
    public decimal Stake { get; }
    public bool Processed { get; }

    public ThingFundedEvent(long blockNumber, string thingIdHash, string userId, decimal stake, bool processed = false)
    {
        BlockNumber = blockNumber;
        ThingIdHash = thingIdHash;
        UserId = userId;
        Stake = stake;
        Processed = processed;
    }
}