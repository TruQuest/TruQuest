using Application.Common.Models.QM;

namespace Application.Thing.Queries.GetThing;

public class ThingQm
{
    public required Guid Id { get; init; }
    public required ThingStateQm State { get; init; }
    public required long? SubmittedAt { get; init; }
    public required string Title { get; init; }
    public required string Details { get; init; }
    public required string? ImageIpfsCid { get; init; }
    public required string? CroppedImageIpfsCid { get; init; }
    public required string SubmitterId { get; init; }
    public required string SubmitterWalletAddress { get; init; }
    public required Guid SubjectId { get; init; }
    public required string SubjectName { get; init; }
    public required string SubjectCroppedImageIpfsCid { get; init; }
    public required int SubjectAvgScore { get; init; }
    public required long? SettledAt { get; init; }
    public required string? VoteAggIpfsCid { get; init; }
    public required Guid? AcceptedSettlementProposalId { get; init; }
    public HashSet<ThingEvidenceQm> Evidence { get; } = new();
    public HashSet<TagQm> Tags { get; } = new();
    public bool Watched { get; set; }
    public required Dictionary<string, string>? RelatedThings { get; init; }

    public override bool Equals(object? obj)
    {
        var other = obj as ThingQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
