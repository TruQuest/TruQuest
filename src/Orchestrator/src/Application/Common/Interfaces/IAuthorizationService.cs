using Domain.Errors;

using Application.Common.Attributes;

namespace Application.Common.Interfaces;

public interface IAuthorizationService
{
    Task<AuthorizationError?> Authorize(
        IEnumerable<RequireAuthorizationAttribute> authorizeAttributes
    );
}