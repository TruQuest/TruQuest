namespace Application.Admin.Queries.GetContractsStates.QM;

public class OrchestratorLotteryCommitmentQm
{
    public required string DataHash { get; init; }
    public required string UserXorDataHash { get; init; }
    public required long Block { get; init; }
}
