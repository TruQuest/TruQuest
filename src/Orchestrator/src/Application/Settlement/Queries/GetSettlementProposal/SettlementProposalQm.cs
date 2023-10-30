using Domain.Aggregates;

namespace Application.Settlement.Queries.GetSettlementProposal;

public class SettlementProposalQm
{
    public Guid Id { get; }
    public Guid ThingId { get; }
    public SettlementProposalState State { get; }
    public long? SubmittedAt { get; }
    public string Title { get; }
    public Verdict Verdict { get; }
    public string Details { get; }
    public string? ImageIpfsCid { get; }
    public string? CroppedImageIpfsCid { get; }
    public string SubmitterId { get; }
    public long? AssessmentPronouncedAt { get; }
    public string SubjectName { get; }
    public string ThingTitle { get; }
    public string? ThingCroppedImageIpfsCid { get; }
    public HashSet<SettlementProposalEvidenceQm> Evidence { get; } = new();

    public bool Watched { get; set; }

    public override bool Equals(object? obj)
    {
        var other = obj as SettlementProposalQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
