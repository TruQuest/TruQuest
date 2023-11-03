using Domain.Aggregates;

namespace Infrastructure.Kafka.Events;

internal class ThingUpdateEvent : TraceableEvent
{
    public required Guid ThingId { get; init; }
    public required ThingUpdateCategory Category { get; init; }
    public required long UpdateTimestamp { get; init; }
    public required string Title { get; init; }
    public required string? Details { get; init; }
}
