using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Repositories;

internal static class IdentityResultExtension
{
    public static Dictionary<string, string[]> ToErrorDictionary(
        this IdentityResult result
    ) => result.Errors.ToDictionary(
        error => error.Code,
        error => new[] { error.Description }
    );
}