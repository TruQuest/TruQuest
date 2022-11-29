using Domain.Errors;
using Domain.Results;

using Application.Account.Commands.SignUp;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.SubmitNewThing;

namespace Application.Common.Interfaces;

public interface ISigner
{
    Either<AccountError, string> RecoverFromSignUpMessage(SignUpIm input, string signature);
    Either<SubjectError, string> RecoverFromNewSubjectMessage(NewSubjectIm input, string signature);
    Either<ThingError, string> RecoverFromNewThingMessage(NewThingIm input, string signature);

    string SignThing(ThingVm thing);
}