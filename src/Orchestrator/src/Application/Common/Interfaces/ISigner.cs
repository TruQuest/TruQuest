using Domain.Errors;
using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.User.Commands.SignUp;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.SubmitNewThing;
using Application.Vote.Commands.CastAcceptancePollVote;
using Application.Settlement.Commands.SubmitNewSettlementProposal;

namespace Application.Common.Interfaces;

public interface ISigner
{
    Either<UserError, string> RecoverFromSignUpMessage(SignUpIm input, string signature);
    Either<SubjectError, string> RecoverFromNewSubjectMessage(NewSubjectIm input, string signature);
    Either<ThingError, string> RecoverFromNewThingMessage(NewThingIm input, string signature);
    Either<VoteError, string> RecoverFromNewVoteMessage(NewVoteIm input, string signature);
    Either<SettlementError, string> RecoverFromNewSettlementProposalMessage(
        NewSettlementProposalIm input, string signature
    );

    string SignThing(ThingVm thing);
    string SignNewVote(NewVoteIm input, string voterId, string voterSignature);
    string SignVoteAgg(IEnumerable<AcceptancePollVote> offChainVotes, IEnumerable<CastedAcceptancePollVoteEvent> onChainVotes);
    string SignSettlementProposal(SettlementProposalVm proposal);
    string SignVoteAgg(
        IEnumerable<AssessmentPollVote> offChainVotes, IEnumerable<CastedAssessmentPollVoteEvent> onChainVotes
    );
}