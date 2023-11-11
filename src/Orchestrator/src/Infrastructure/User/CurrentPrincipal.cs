using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Application.Common.Interfaces;

namespace Infrastructure.User;

internal class CurrentPrincipal : ICurrentPrincipal
{
    private readonly ClaimsPrincipal? _principal;

    public CurrentPrincipal(IAuthenticationContext authenticationContext)
    {
        _principal = authenticationContext.User;
    }

    public string? Id => _principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
    public string? SignerAddress => _principal?.FindFirstValue("signer_address");
    public string? WalletAddress => _principal?.FindFirstValue("wallet_address");
    public bool IsAdmin => _principal?.HasClaim("is_admin", "true") ?? false;
}
