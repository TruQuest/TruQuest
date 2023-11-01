using Application.Common.Models.QM;
using Application.Subject.Common.Models.QM;

namespace Application.Subject.Queries.GetSubjects;

public class SubjectPreviewQm
{
    public required Guid Id { get; init; }
    public required long SubmittedAt { get; init; }
    public required string Name { get; init; }
    public required SubjectTypeQm Type { get; init; }
    public required string CroppedImageIpfsCid { get; init; }
    public required int SettledThingsCount { get; init; }
    public required int AvgScore { get; init; }

    public HashSet<TagQm> Tags { get; } = new();

    public override bool Equals(object? obj)
    {
        var other = obj as SubjectPreviewQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
