using Application.Common.Models.QM;
using Application.Subject.Common.Models.QM;

namespace Application.Subject.Queries.GetSubjects;

public class SubjectPreviewQm
{
    public Guid Id { get; }
    public long SubmittedAt { get; }
    public string Name { get; }
    public SubjectTypeQm Type { get; }
    public string CroppedImageIpfsCid { get; }
    public string SubmitterId { get; }
    public int? SettledThingsCount { get; }
    public int? AvgScore { get; }

    public HashSet<TagQm> Tags { get; } = new();

    public override bool Equals(object? obj)
    {
        var other = obj as SubjectPreviewQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}