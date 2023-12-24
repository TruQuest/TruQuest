namespace Application.Admin.Queries.GetContractsStates.QM;

public class ThingSubmitterQm
{
    public required Guid ThingId { get; init; }
    public required string Submitter { get; init; }
}
