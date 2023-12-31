using System.Security.Claims;

using Microsoft.IdentityModel.Tokens;

namespace Application.Common.Interfaces;

public interface IAuthenticationContext
{
    ClaimsPrincipal? User { get; set; }
    SecurityToken? Token { get; set; }
    Exception? Failure { get; set; }

    void SetValuesFrom(IAuthenticationContext other);
    string GetFailureMessage();
}
