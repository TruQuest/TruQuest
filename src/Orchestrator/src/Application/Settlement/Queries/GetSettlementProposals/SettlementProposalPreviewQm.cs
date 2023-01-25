using Domain.Aggregates;

namespace Application.Settlement.Queries.GetSettlementProposals;

public class SettlementProposalPreviewQm
{
    public required Guid Id { get; init; }
    public required SettlementProposalState State { get; init; }
    public required string Title { get; init; }
    public required Verdict Verdict { get; init; }
    public required string? CroppedImageIpfsCid { get; init; }
    public required string SubmitterId { get; init; }
}