namespace Infrastructure.Ethereum.TypedData;

public class OnChainThingValidationPollVoteTd
{
    public required string TxnHash { get; init; }
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required int LogIndex { get; init; }
    public required string UserId { get; init; }
    public required string WalletAddress { get; init; }
    public required string Decision { get; init; }
    public required string Reason { get; init; }
}
