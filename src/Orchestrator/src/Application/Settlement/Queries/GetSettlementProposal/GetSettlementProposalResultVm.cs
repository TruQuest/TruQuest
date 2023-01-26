namespace Application.Settlement.Queries.GetSettlementProposal;

public class GetSettlementProposalResultVm
{
    public required SettlementProposalQm Proposal { get; init; }
    public required string? Signature { get; init; }
}