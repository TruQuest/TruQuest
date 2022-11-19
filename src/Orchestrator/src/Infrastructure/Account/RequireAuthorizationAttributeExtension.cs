using Microsoft.AspNetCore.Authorization;

using Application.Common.Attributes;

namespace Infrastructure.Account;

public static class RequireAuthorizationAttributeExtension
{
    public static IEnumerable<IAuthorizeData> ToAuthorizeData(
        this IEnumerable<RequireAuthorizationAttribute> authorizeAttributes
    ) => authorizeAttributes.Select(
        a => new AuthorizeAttribute { Policy = a.Policy, Roles = a.Roles }
    );
}