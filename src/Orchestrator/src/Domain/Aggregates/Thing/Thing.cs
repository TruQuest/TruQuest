using Domain.Base;

namespace Domain.Aggregates;

public class Thing : Entity, IAggregateRoot
{
    public Guid? Id { get; private set; }
    public string? IdHash { get; private set; }
    public string Title { get; }
    public string Details { get; }
    public string? ImageURL { get; }
    public string SubmitterId { get; }
    public Guid SubjectId { get; }

    private List<Evidence> _evidence = new();
    public IReadOnlyList<Evidence> Evidence => _evidence;

    private List<ThingAttachedTag> _tags = new();
    public IReadOnlyList<ThingAttachedTag> Tags => _tags;

    public Thing(string title, string details, string? imageURL, string submitterId, Guid subjectId)
    {
        Title = title;
        Details = details;
        ImageURL = imageURL;
        SubmitterId = submitterId;
        SubjectId = subjectId;
    }

    public void AddEvidence(IEnumerable<Evidence> evidence)
    {
        _evidence.AddRange(evidence);
    }

    public void AddTags(IEnumerable<int> tagIds)
    {
        _tags.AddRange(tagIds.Select(tagId => new ThingAttachedTag(tagId)));
    }
}