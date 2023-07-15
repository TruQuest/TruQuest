using UserDm = Domain.Aggregates.User;

namespace Application.Common.Interfaces;

public interface IConfirmationTokenProvider
{
    Task<string> GetEmailConfirmationToken(UserDm user);
    Task<bool> VerifyEmailConfirmationToken(UserDm user, string token);
}
