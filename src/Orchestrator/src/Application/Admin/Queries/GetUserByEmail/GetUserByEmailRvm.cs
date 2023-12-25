namespace Application.Admin.Queries.GetUserByEmail;

public class GetUserByEmailRvm
{
    public required string UserId { get; init; }
    public required string SignerAddress { get; init; }
    public required string WalletAddress { get; init; }
    public required bool EmailConfirmed { get; init; }
}
