namespace Application.Admin.Queries.GetContractsStates.VM;

public class SubjectVm
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required IEnumerable<ThingVm> Things { get; init; }
}
