using Application.Common.Attributes;

namespace Application.Settlement.Commands.SubmitNewSettlementProposal;

public class SupportingEvidenceIm
{
    [WebPageUrl(
        BackingField = nameof(HtmlIpfsCid),
        ExtraBackingField = nameof(JpgIpfsCid)
    )]
    public string Url { get; set; }

    internal string? HtmlIpfsCid { get; set; }
    internal string? JpgIpfsCid { get; set; }
}