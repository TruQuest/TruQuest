using System.Globalization;

using Microsoft.AspNetCore.Authorization;

using Domain.Errors;
using Application.Common.Attributes;
using AppInterfaces = Application.Common.Interfaces;

namespace Infrastructure.User;

public class AuthorizationService : AppInterfaces.IAuthorizationService
{
    private readonly AppInterfaces.IAuthenticationContext _authenticationContext;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthorizationPolicyProvider _authorizationPolicyProvider;

    public AuthorizationService(
        AppInterfaces.IAuthenticationContext authenticationContext,
        IAuthorizationService authorizationService,
        IAuthorizationPolicyProvider authorizationPolicyProvider
    )
    {
        _authenticationContext = authenticationContext;
        _authorizationService = authorizationService;
        _authorizationPolicyProvider = authorizationPolicyProvider;
    }

    public async Task<AuthorizationError?> Authorize(
        IEnumerable<RequireAuthorizationAttribute> authorizeAttributes
    )
    {
        var authorizeData = authorizeAttributes.ToAuthorizeData();
        var combinedPolicy = await AuthorizationPolicy.CombineAsync(
            _authorizationPolicyProvider, authorizeData
        );
        if (combinedPolicy == null)
        {
            return null;
        }

        if (_authenticationContext.User == null)
        {
            return new AuthorizationError(_authenticationContext.GetFailureMessage());
        }

        // @@NOTE: Token is set when SignalR hub path is requested, authentication is a success, and the
        // invoked hub method is decorated with [CopyAuthenticationContextToMethodInvocationScope] attribute.
        if (_authenticationContext.Token != null)
        {
            // @@NOTE: Authentication in SignalR (which includes token lifetime validation) is performed once
            // per connection, not once per method call. Therefore, it's possible for a stolen token
            // to be used indefinitely, even long after it should have been discarded as expired.
            // To solve this problem we save the token in IAuthenticationContext and validate its lifetime
            // for every hub method invocation manually.
            if (DateTime.UtcNow.CompareTo(_authenticationContext.Token.ValidTo) > 0)
            {
                return new AuthorizationError(
                    $"The token expired at '{_authenticationContext.Token.ValidTo.ToString(CultureInfo.InvariantCulture)}'"
                );
            }
        }

        var result = await _authorizationService.AuthorizeAsync(
            _authenticationContext.User, combinedPolicy
        );

        return !result.Succeeded ? new AuthorizationError("Forbidden") : null;
    }
}