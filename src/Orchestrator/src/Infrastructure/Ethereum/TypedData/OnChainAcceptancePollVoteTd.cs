namespace Infrastructure.Ethereum.TypedData;

public class OnChainAcceptancePollVoteTd
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required long L1BlockNumber { get; init; }
    public required string UserId { get; init; }
    public required string Decision { get; init; }
    public required string Reason { get; init; }
}
