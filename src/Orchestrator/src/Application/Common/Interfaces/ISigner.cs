using Domain.Errors;
using Domain.Results;

using Application.Account.Commands.SignUp;
using Application.Subject.Commands.AddNewSubject;

namespace Application.Common.Interfaces;

public interface ISigner
{
    Either<AccountError, string> RecoverFromSignUpMessage(SignUpIM input, string signature);
    Either<SubjectError, string> RecoverFromNewSubjectMessage(NewSubjectIM input, string signature);
}