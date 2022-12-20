using Application.Common.Attributes;

namespace Application.Settlement.Commands.SubmitNewSettlementProposal;

public class SupportingEvidenceIm
{
    [WebPageUrl(KeepOriginUrl = true)]
    public string Url { get; set; }
}