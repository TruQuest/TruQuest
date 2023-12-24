namespace Application.Admin.Queries.GetContractsStates.QM;

public class SettlementProposalAssessmentPollQm
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required long InitBlockNumber { get; init; }
    public required IEnumerable<string> Verifiers { get; init; }
}
