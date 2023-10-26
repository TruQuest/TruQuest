namespace Application.Ethereum.Common.Models.IM;

public abstract class BaseContractEvent
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required string TxnHash { get; init; }
}
