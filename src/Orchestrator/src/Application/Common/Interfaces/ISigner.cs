using Domain.Errors;
using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.User.Commands.SignUp;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.SubmitNewThing;
using Application.Thing.Commands.CastAcceptancePollVote;
using Application.Settlement.Commands.SubmitNewSettlementProposal;

namespace Application.Common.Interfaces;

public interface ISigner
{
    Either<UserError, string> RecoverFromSignUpMessage(SignUpIm input, string signature);
    Either<SubjectError, string> RecoverFromNewSubjectMessage(NewSubjectIm input, string signature);
    Either<ThingError, string> RecoverFromNewThingMessage(NewThingIm input, string signature);
    Either<VoteError, string> RecoverFromNewAcceptancePollVoteMessage(NewAcceptancePollVoteIm input, string signature);
    Either<SettlementError, string> RecoverFromNewSettlementProposalMessage(
        NewSettlementProposalIm input, string signature
    );
    bool CheckOrchestratorSignatureOnTimestamp(string timestamp, string signature);
    string RecoverFromSignInMessage(string timestamp, string orchestratorSignature, string signature);

    string SignThing(Guid thingId);
    string SignNewAcceptancePollVote(NewAcceptancePollVoteIm input, string voterId, string voterSignature);
    string SignAcceptancePollVoteAgg(IEnumerable<AcceptancePollVote> offChainVotes, IEnumerable<CastedAcceptancePollVoteEvent> onChainVotes);
    string SignSettlementProposal(SettlementProposalVm proposal);
    string SignAssessmentPollVoteAgg(
        IEnumerable<AssessmentPollVote> offChainVotes, IEnumerable<CastedAssessmentPollVoteEvent> onChainVotes
    );
    string SignTimestamp(DateTimeOffset timestamp);
}