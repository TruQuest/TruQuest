using Domain.Base;

namespace Domain.Aggregates;

public class ThingUpdate : Entity, IAggregateRoot
{
    public Guid ThingId { get; }
    public long UpdateTimestamp { get; }
    public string Title { get; }
    public string? Details { get; }

    public ThingUpdate(Guid thingId, long updateTimestamp, string title, string? details = null)
    {
        ThingId = thingId;
        UpdateTimestamp = updateTimestamp;
        Title = title;
        Details = details;
    }
}