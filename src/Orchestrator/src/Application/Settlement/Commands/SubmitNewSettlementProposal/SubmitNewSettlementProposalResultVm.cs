namespace Application.Settlement.Commands.SubmitNewSettlementProposal;

public class SubmitNewSettlementProposalResultVm
{
    public required Guid ThingId { get; init; }
    public required Guid ProposalId { get; init; }
    public required string Signature { get; init; }
}