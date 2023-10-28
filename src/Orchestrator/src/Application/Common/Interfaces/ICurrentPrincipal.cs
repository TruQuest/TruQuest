namespace Application.Common.Interfaces;

public interface ICurrentPrincipal
{
    string? Id { get; }
    string? SignerAddress { get; }
    string? WalletAddress { get; }
}
