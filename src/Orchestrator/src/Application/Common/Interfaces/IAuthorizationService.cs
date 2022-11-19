using Domain.Errors;

using Application.Common.Attributes;

namespace Application.Common.Interfaces;

public interface IAuthorizationService
{
    Task<AuthorizationError?> Authorize(
        IAuthenticationContext authenticationContext,
        IEnumerable<RequireAuthorizationAttribute> authorizeAttributes
    );
}