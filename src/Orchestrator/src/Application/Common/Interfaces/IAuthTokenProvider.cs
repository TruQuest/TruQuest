using System.Security.Claims;

namespace Application.Common.Interfaces;

public interface IAuthTokenProvider
{
    string GenerateJWT(string id, IList<Claim>? claims = null);
}