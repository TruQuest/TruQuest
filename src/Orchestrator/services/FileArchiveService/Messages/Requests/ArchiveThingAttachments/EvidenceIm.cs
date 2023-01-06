using Attributes;

namespace Messages.Requests;

internal class EvidenceIm
{
    [WebPageUrl(
        BackingField = nameof(IpfsCid),
        ExtraBackingField = nameof(PreviewImageIpfsCid)
    )]
    public string Url { get; set; }

    [BackingField]
    public string? IpfsCid { get; set; }
    [BackingField]
    public string? PreviewImageIpfsCid { get; set; }
}