namespace Infrastructure.Ethereum.TypedData;

public class SignedNewAcceptancePollVoteTd
{
    public required string Vote { get; init; }
    public required string WalletAddress { get; init; }
    public required string OwnerAddress { get; init; }
    public required string OwnerSignature { get; init; }
}
