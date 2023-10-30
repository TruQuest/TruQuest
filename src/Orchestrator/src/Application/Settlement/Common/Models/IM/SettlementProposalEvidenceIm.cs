namespace Application.Settlement.Common.Models.IM;

public class SettlementProposalEvidenceIm
{
    public required string Url { get; init; }
    public string? IpfsCid { get; init; }
    public string? PreviewImageIpfsCid { get; init; }
}
