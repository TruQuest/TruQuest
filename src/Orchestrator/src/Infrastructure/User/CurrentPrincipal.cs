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

    public string? Username => _principal?.FindFirstValue("username")!;
}