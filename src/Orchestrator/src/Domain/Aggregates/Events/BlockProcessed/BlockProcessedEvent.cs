using Domain.Base;

namespace Domain.Aggregates.Events;

public class BlockProcessedEvent : Entity
{
    public long Id { get; }
    public long? BlockNumber { get; }

    public BlockProcessedEvent(long id, long? blockNumber)
    {
        Id = id;
        BlockNumber = blockNumber;
    }
}