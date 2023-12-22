namespace Application.General.Queries.GetContractsStates.VM;

public class LotteryParticipantVm
{
    public required string UserId { get; init; }
    public required string WalletAddress { get; init; }
    public required long JoinedBlockNumber { get; init; }
}
