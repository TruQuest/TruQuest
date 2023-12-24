namespace Application.Admin.Queries.GetContractsStates.QM;

public class SettlementProposalSubmitterQm
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required string Submitter { get; init; }
}
