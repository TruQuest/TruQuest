using Domain.Aggregates;

namespace Application.Settlement.Queries.GetSettlementProposal;

public class SettlementProposalQm
{
    public required Guid Id { get; init; }
    public required Guid ThingId { get; init; }
    public required SettlementProposalState State { get; init; }
    public required long? SubmittedAt { get; init; }
    public required string Title { get; init; }
    public required Verdict Verdict { get; init; }
    public required string Details { get; init; }
    public required string? ImageIpfsCid { get; init; }
    public required string? CroppedImageIpfsCid { get; init; }
    public required string SubmitterId { get; init; }
    public required string SubmitterWalletAddress { get; init; }
    public required long? AssessmentPronouncedAt { get; init; }
    public required string SubjectName { get; init; }
    public required string ThingTitle { get; init; }
    public required string? ThingCroppedImageIpfsCid { get; init; }
    public HashSet<SettlementProposalEvidenceQm> Evidence { get; } = new();
    public required Dictionary<string, string>? RelatedProposals { get; init; }

    public bool Watched { get; set; }

    public override bool Equals(object? obj)
    {
        var other = obj as SettlementProposalQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
