using Microsoft.AspNetCore.Identity;

using UserDm = Domain.Aggregates.User;
using Application.Common.Interfaces;

namespace Infrastructure.User;

internal class ConfirmationTokenProvider : IConfirmationTokenProvider
{
    private readonly UserManager<UserDm> _userManager;

    public ConfirmationTokenProvider(UserManager<UserDm> userManager)
    {
        _userManager = userManager;
    }

    public Task<string> GetEmailConfirmationToken(UserDm user) =>
        _userManager.GenerateEmailConfirmationTokenAsync(user);

    public Task<bool> VerifyEmailConfirmationToken(UserDm user, string token) =>
        _userManager.VerifyUserTokenAsync(
            user,
            tokenProvider: "Default",
            purpose: "EmailConfirmation",
            token: token
        );
}
