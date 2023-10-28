namespace Infrastructure.Ethereum.TypedData;

public class SignedNewAssessmentPollVoteTd
{
    public required string Vote { get; init; }
    public required string UserId { get; init; }
    public required string WalletAddress { get; init; }
    public required string SignerAddress { get; init; }
    public required string Signature { get; init; }
}
