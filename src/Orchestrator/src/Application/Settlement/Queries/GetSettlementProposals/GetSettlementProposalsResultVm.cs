namespace Application.Settlement.Queries.GetSettlementProposals;

public class GetSettlementProposalsResultVm
{
    public required Guid ThingId { get; init; }
    public required IEnumerable<SettlementProposalPreviewQm> Proposals { get; init; }
}