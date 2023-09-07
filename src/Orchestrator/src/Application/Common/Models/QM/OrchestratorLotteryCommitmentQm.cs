namespace Application.Common.Models.QM;

public class OrchestratorLotteryCommitmentQm
{
    public required long L1BlockNumber { get; init; }
    public required string TxnHash { get; init; }
    public required string DataHash { get; init; }
    public required string UserXorDataHash { get; init; }
}
