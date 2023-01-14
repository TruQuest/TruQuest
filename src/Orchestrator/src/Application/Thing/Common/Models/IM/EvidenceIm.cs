namespace Application.Thing.Common.Models.IM;

public class EvidenceIm
{
    public required string Url { get; init; }
    public string? IpfsCid { get; init; }
    public string? PreviewImageIpfsCid { get; init; }
}