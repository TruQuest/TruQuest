namespace Application.Settlement.Commands.SubmitNewSettlementProposal;

public class SubmitNewSettlementProposalResultVm
{
    public required SettlementProposalVm SettlementProposal { get; init; }
    public required string Signature { get; init; }
}