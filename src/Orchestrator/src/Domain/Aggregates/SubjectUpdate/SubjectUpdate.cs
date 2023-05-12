using Domain.Base;

namespace Domain.Aggregates;

public class SubjectUpdate : Entity, IAggregateRoot
{
    public Guid SubjectId { get; }
    public SubjectUpdateCategory Category { get; }
    public long UpdateTimestamp { get; }
    public string Title { get; }
    public string? Details { get; }

    public SubjectUpdate(
        Guid subjectId, SubjectUpdateCategory category, long updateTimestamp,
        string title, string? details = null
    )
    {
        SubjectId = subjectId;
        Category = category;
        UpdateTimestamp = updateTimestamp;
        Title = title;
        Details = details;
    }
}