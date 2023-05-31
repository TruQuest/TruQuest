using Application.Common.Models.QM;
using Application.Subject.Common.Models.QM;

namespace Application.Subject.Queries.GetSubject;

public class SubjectQm
{
    public Guid Id { get; }
    public long SubmittedAt { get; }
    public string Name { get; }
    public string Details { get; }
    public SubjectTypeQm Type { get; }
    public string ImageIpfsCid { get; }
    public string CroppedImageIpfsCid { get; }
    public string SubmitterId { get; }
    public int SettledThingsCount { get; }
    public int AvgScore { get; }

    public List<TagQm> Tags { get; } = new();

    public List<ThingPreviewQm> LatestSettledThings { get; set; }
    public List<ThingPreviewQm> LatestUnsettledThings { get; set; }

    public override bool Equals(object? obj)
    {
        var other = obj as SubjectQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}