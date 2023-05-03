using Domain.Base;

namespace Domain.Aggregates;

public class Subject : Entity, IAggregateRoot
{
    public Guid? Id { get; private set; }
    public long SubmittedAt { get; }
    public string Name { get; }
    public string Details { get; }
    public SubjectType Type { get; }
    public string ImageIpfsCid { get; }
    public string CroppedImageIpfsCid { get; }
    public string SubmitterId { get; }

    private List<SubjectAttachedTag> _tags = new();
    public IReadOnlyList<SubjectAttachedTag> Tags => _tags;

    public Subject(
        string name, string details, SubjectType type, string imageIpfsCid,
        string croppedImageIpfsCid, string submitterId
    )
    {
        SubmittedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Name = name;
        Details = details;
        Type = type;
        ImageIpfsCid = imageIpfsCid;
        CroppedImageIpfsCid = croppedImageIpfsCid;
        SubmitterId = submitterId;
    }

    public void AddTags(IEnumerable<int> tagIds)
    {
        _tags.AddRange(tagIds.Select(tagId => new SubjectAttachedTag(tagId)));
    }
}