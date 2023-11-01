using Application.Common.Models.QM;
using Application.Subject.Common.Models.QM;

namespace Application.Subject.Queries.GetSubject;

public class SubjectQm
{
    public required Guid Id { get; init; }
    public required long SubmittedAt { get; init; }
    public required string Name { get; init; }
    public required string Details { get; init; }
    public required SubjectTypeQm Type { get; init; }
    public required string ImageIpfsCid { get; init; }
    public required string CroppedImageIpfsCid { get; init; }
    public required string SubmitterId { get; init; }
    public required string SubmitterWalletAddress { get; init; }
    public required int SettledThingsCount { get; init; }
    public required int AvgScore { get; init; }

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
