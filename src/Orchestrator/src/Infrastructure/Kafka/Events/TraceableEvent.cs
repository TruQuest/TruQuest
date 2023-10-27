namespace Infrastructure.Kafka.Events;

internal abstract class TraceableEvent
{
    public required string Traceparent { get; init; }
}
