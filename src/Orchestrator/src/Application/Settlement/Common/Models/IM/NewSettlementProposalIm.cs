namespace Application.Settlement.Common.Models.IM;

public class NewSettlementProposalIm
{
    public required Guid ThingId { get; init; }
    public required string Title { get; init; }
    public required VerdictIm Verdict { get; init; }
    public required string Details { get; init; }
    public required string? ImagePath { get; init; }
    public required string? CroppedImagePath { get; init; }
    public required IEnumerable<SupportingEvidenceIm> Evidence { get; init; }

    public string? ImageIpfsCid { get; init; }
    public string? CroppedImageIpfsCid { get; init; }
}