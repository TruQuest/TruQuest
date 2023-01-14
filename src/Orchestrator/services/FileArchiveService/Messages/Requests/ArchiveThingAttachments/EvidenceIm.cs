using Attributes;

namespace Messages.Requests;

internal class EvidenceIm
{
    [WebPageUrl(
        BackingField = nameof(IpfsCid),
        ExtraBackingField = nameof(PreviewImageIpfsCid)
    )]
    public required string Url { get; init; }

    [BackingField]
    public required string? IpfsCid { get; set; }
    [BackingField]
    public required string? PreviewImageIpfsCid { get; set; }
}