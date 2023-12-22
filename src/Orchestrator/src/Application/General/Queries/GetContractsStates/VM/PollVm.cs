namespace Application.General.Queries.GetContractsStates.VM;

public class PollVm
{
    public required long InitBlockNumber { get; init; }
    public required IEnumerable<VerifierVm> Verifiers { get; init; }
}
