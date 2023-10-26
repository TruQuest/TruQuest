namespace Infrastructure.Kafka.Messages;

internal abstract class TraceableEvent
{
    public required string Traceparent { get; init; }
}
