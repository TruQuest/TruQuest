using Application.Common.Attributes;

namespace Application.Thing.Commands.SubmitNewThing;

public class EvidenceIm
{
    [WebPageUrl(
        BackingField = nameof(HtmlIpfsCid),
        ExtraBackingField = nameof(JpgIpfsCid)
    )]
    public string Url { get; set; }

    internal string? HtmlIpfsCid { get; set; }
    internal string? JpgIpfsCid { get; set; }
}