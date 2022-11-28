using Domain.Errors;
using Domain.Results;

using Application.Account.Commands.SignUp;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.SubmitNewThing;

namespace Application.Common.Interfaces;

public interface ISigner
{
    Either<AccountError, string> RecoverFromSignUpMessage(SignUpIM input, string signature);
    Either<SubjectError, string> RecoverFromNewSubjectMessage(NewSubjectIM input, string signature);
    Either<ThingError, string> RecoverFromNewThingMessage(NewThingIM input, string signature);

    string SignThing(ThingVM thing);
}