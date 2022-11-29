using System.Security.Claims;

namespace Application.Common.Interfaces;

public interface IAuthTokenProvider
{
    string GenerateJwt(string id, IList<Claim>? claims = null);
}