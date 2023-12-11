using KafkaFlow;

namespace Infrastructure.Kafka.Events;

internal abstract class TraceableEvent
{
    public required string Traceparent { get; init; }

    public virtual IEnumerable<(string Name, object? Value)> GetActivityTags(IMessageContext context) =>
        Enumerable.Empty<(string Name, object? Value)>();
}
