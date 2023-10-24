namespace Application.User.Common.Models.VM;

public class AuthResultVm
{
    public required string UserId { get; init; }
    public required string SignerAddress { get; init; }
    public required string WalletAddress { get; init; }
    public required string Token { get; init; }
}
