namespace Application.Thing.Queries.GetSettlementProposalsList;

public class GetSettlementProposalsListResultVm
{
    public required Guid ThingId { get; init; }
    public required IEnumerable<SettlementProposalPreviewQm> Proposals { get; init; }
}