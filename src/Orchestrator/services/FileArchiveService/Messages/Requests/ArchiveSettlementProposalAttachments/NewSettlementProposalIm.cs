using Attributes;

namespace Messages.Requests;

internal class NewSettlementProposalIm
{
    public required Guid ThingId { get; init; }
    public required string Title { get; init; }
    public required VerdictIm Verdict { get; init; }
    public required string Details { get; init; }
    [ImagePath(BackingField = nameof(ImageIpfsCid))]
    public required string? ImagePath { get; init; }
    [ImagePath(BackingField = nameof(CroppedImageIpfsCid))]
    public required string? CroppedImagePath { get; init; }
    public required IEnumerable<SupportingEvidenceIm> Evidence { get; init; }

    [BackingField]
    public required string? ImageIpfsCid { get; set; }
    [BackingField]
    public required string? CroppedImageIpfsCid { get; set; }
}