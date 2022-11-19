using System.Globalization;

using Microsoft.AspNetCore.Authorization;

using Domain.Errors;
using Application.Common.Attributes;
using AppInterfaces = Application.Common.Interfaces;

namespace Infrastructure.Account;

public class AuthorizationService : AppInterfaces.IAuthorizationService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthorizationPolicyProvider _authorizationPolicyProvider;

    public AuthorizationService(
        IAuthorizationService authorizationService,
        IAuthorizationPolicyProvider authorizationPolicyProvider
    )
    {
        _authorizationService = authorizationService;
        _authorizationPolicyProvider = authorizationPolicyProvider;
    }

    public async Task<AuthorizationError?> Authorize(
        AppInterfaces.IAuthenticationContext authenticationContext,
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

        if (authenticationContext.User == null)
        {
            return new AuthorizationError(authenticationContext.GetFailureMessage());
        }

        // @@NOTE: Token is set when SignalR hub path is requested, authentication is a success, and the
        // invoked hub method is decorated with [CopyAuthenticationContextToMethodInvocationScope] attribute.
        if (authenticationContext.Token != null)
        {
            // @@NOTE: Authentication in SignalR (which includes token lifetime validation) is performed once
            // per connection, not once per method call. Therefore, it's possible for a stolen token
            // to be used indefinitely, even long after it should have been discarded as expired.
            // To solve this problem we save the token in IAuthenticationContext and validate its lifetime
            // for every hub method invocation manually.
            if (DateTime.UtcNow.CompareTo(authenticationContext.Token.ValidTo) > 0)
            {
                return new AuthorizationError(
                    $"The token expired at '{authenticationContext.Token.ValidTo.ToString(CultureInfo.InvariantCulture)}'"
                );
            }
        }

        var result = await _authorizationService.AuthorizeAsync(
            authenticationContext.User, combinedPolicy
        );

        return !result.Succeeded ? new AuthorizationError("Forbidden") : null;
    }
}