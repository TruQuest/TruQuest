using Domain.Base;

namespace Domain.Aggregates;

public class DeadLetter : Entity, IAggregateRoot
{
    public int? Id { get; private set; }
    public DeadLetterSource Source { get; }
    public long ArchivedAt { get; }
    public DeadLetterState State { get; }
    private Dictionary<string, object> _payload;
    public IReadOnlyDictionary<string, object> Payload => _payload;

    public DeadLetter(DeadLetterSource source, long archivedAt)
    {
        Source = source;
        ArchivedAt = archivedAt;
        State = DeadLetterState.Unhandled;
    }

    public void SetPayload(Dictionary<string, object> payload) => _payload = payload;
}
