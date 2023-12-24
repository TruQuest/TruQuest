namespace Application.Admin.Queries.GetContractsStates.QM;

public class ThingValidationPollQm
{
    public required Guid ThingId { get; init; }
    public required long InitBlockNumber { get; init; }
    public required IEnumerable<string> Verifiers { get; init; }
}
