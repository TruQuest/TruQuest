namespace Application.General.Queries.GetContractsStates.VM;

public class UserVm
{
    public required string Id { get; init; }
    public required string WalletAddress { get; init; }
    public required string HexBalance { get; init; }
    public required string HexStakedBalance { get; init; }
}
