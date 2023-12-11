using KafkaFlow;

using Domain.Aggregates;
using Application.Common.Monitoring;

namespace Infrastructure.Kafka.Events;

internal class ThingUpdateEvent : TraceableEvent
{
    public required Guid ThingId { get; init; }
    public required ThingUpdateCategory Category { get; init; }
    public required long UpdateTimestamp { get; init; }
    public required string Title { get; init; }
    public required string? Details { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags(IMessageContext _)
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.ThingId, ThingId),
            (ActivityTags.ItemUpdateCategory, Category.ToString()),
        };
    }
}
