using Domain.Base;

namespace Domain.Aggregates;

public class Subject : Entity, IAggregateRoot
{
    public Guid? Id { get; private set; }

    public string Name { get; }
    public string Details { get; }
    public int Type { get; }
    public string? ImageURL { get; }
    public string SubmitterId { get; }

    private List<SubjectAttachedTag> _tags = new();
    public IReadOnlyList<SubjectAttachedTag> Tags => _tags;

    public Subject(string name, string details, int type, string? imageURL, string submitterId)
    {
        Name = name;
        Details = details;
        Type = type;
        ImageURL = imageURL;
        SubmitterId = submitterId;
    }

    public void AddTags(IEnumerable<int> tagIds)
    {
        _tags.AddRange(tagIds.Select(tagId => new SubjectAttachedTag(tagId)));
    }
}