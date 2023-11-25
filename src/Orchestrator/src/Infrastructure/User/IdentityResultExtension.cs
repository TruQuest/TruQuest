using Microsoft.AspNetCore.Identity;

namespace Infrastructure.User;

internal static class IdentityResultExtension
{
    public static string ToErrorMessage(this IdentityResult result) =>
        string.Join(";\n", result.Errors.Select(e => $"[{e.Code}]: {e.Description}"));
}
