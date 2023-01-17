using Application.Common.Models.QM;

namespace Application.Thing.Queries.GetThing;

public class ThingQm
{
    public Guid Id { get; }
    public ThingStateQm State { get; }
    public string Title { get; }
    public string Details { get; }
    public string? ImageIpfsCid { get; }
    public string? CroppedImageIpfsCid { get; }
    public string SubmitterId { get; }
    public Guid SubjectId { get; }
    public HashSet<EvidenceQm> Evidence { get; } = new();
    public HashSet<TagQm> Tags { get; } = new();

    public override bool Equals(object? obj)
    {
        var other = obj as ThingQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}