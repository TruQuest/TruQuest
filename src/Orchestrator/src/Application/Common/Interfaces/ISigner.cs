using Domain.Errors;
using Domain.Results;
using Domain.Aggregates.Events;
using VoteDm = Domain.Aggregates.Vote;

using Application.User.Commands.SignUp;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.SubmitNewThing;
using Application.Vote.Commands.CastVote;

namespace Application.Common.Interfaces;

public interface ISigner
{
    Either<UserError, string> RecoverFromSignUpMessage(SignUpIm input, string signature);
    Either<SubjectError, string> RecoverFromNewSubjectMessage(NewSubjectIm input, string signature);
    Either<ThingError, string> RecoverFromNewThingMessage(NewThingIm input, string signature);
    Either<VoteError, string> RecoverFromNewVoteMessage(NewVoteIm input, string signature);

    string SignThing(ThingVm thing);
    string SignNewVote(NewVoteIm input, string voterId, string voterSignature);
    string SignVoteAgg(IEnumerable<VoteDm> offChainVotes, IEnumerable<CastedAcceptancePollVoteEvent> onChainVotes);
}