using Domain.Base;

namespace Domain.Aggregates.Events;

public class ActionableThingRelatedEvent : Entity, IAggregateRoot
{
    public Guid? Id { get; private set; }
    public long BlockNumber { get; }
    public int TxnIndex { get; }
    public Guid ThingId { get; }
    public ThingEventType Type { get; }
    private Dictionary<string, object> _payload = new();
    public IReadOnlyDictionary<string, object> Payload => _payload;

    public ActionableThingRelatedEvent(long blockNumber, int txnIndex, Guid thingId, ThingEventType type)
    {
        BlockNumber = blockNumber;
        TxnIndex = txnIndex;
        ThingId = thingId;
        Type = type;
    }

    public void SetPayload(Dictionary<string, object> payload)
    {
        _payload = payload;
    }
}