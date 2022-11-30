using Domain.Base;

namespace Domain.Aggregates;

public class DeferredTask : Entity, IAggregateRoot
{
    public long? Id { get; private set; }
    public TaskType Type { get; }
    public long ScheduledBlockNumber { get; }
    private Dictionary<string, object> _payload = new();
    public IReadOnlyDictionary<string, object> Payload => _payload;

    public DeferredTask(TaskType type, long scheduledBlockNumber)
    {
        Type = type;
        ScheduledBlockNumber = scheduledBlockNumber;
    }

    public void SetPayload(Dictionary<string, object> payload)
    {
        _payload = payload;
    }
}