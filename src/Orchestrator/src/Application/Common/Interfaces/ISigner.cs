using Domain.Errors;
using Domain.Results;

using Application.Account.Commands.SignUp;

namespace Application.Common.Interfaces;

public interface ISigner
{
    Either<AccountError, string> RecoverFromSignUpMessage(SignUpIM input, string signature);
}