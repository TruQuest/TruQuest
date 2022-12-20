namespace Application.Settlement.Commands.SubmitNewSettlementProposal;

public class NewSettlementProposalIm
{
    public Guid ThingId { get; set; }
    public string Title { get; set; }
    public VerdictIm Verdict { get; set; }
    public string Details { get; set; }
    public IEnumerable<SupportingEvidenceIm> Evidence { get; set; }
}