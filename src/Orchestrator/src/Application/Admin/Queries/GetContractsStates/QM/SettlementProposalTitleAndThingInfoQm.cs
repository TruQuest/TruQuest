namespace Application.Admin.Queries.GetContractsStates.QM;

public class SettlementProposalTitleAndThingInfoQm
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required Guid ThingId { get; init; }
}
