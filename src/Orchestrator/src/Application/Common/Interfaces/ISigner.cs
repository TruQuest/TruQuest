using Domain.Errors;
using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.User.Commands.SignUp;
using Application.Thing.Commands.CastAcceptancePollVote;

namespace Application.Common.Interfaces;

public interface ISigner
{
    Either<UserError, string> RecoverFromSignUpMessage(SignUpIm input, string signature);
    Either<VoteError, string> RecoverFromNewAcceptancePollVoteMessage(NewAcceptancePollVoteIm input, string signature);
    bool CheckOrchestratorSignatureOnTimestamp(string timestamp, string signature);
    string RecoverFromSignInMessage(string timestamp, string orchestratorSignature, string signature);

    string SignThing(Guid thingId);
    string SignNewAcceptancePollVote(NewAcceptancePollVoteIm input, string voterId, string voterSignature);
    string SignAcceptancePollVoteAgg(
        Guid thingId,
        IEnumerable<AcceptancePollVote> offChainVotes,
        IEnumerable<CastedAcceptancePollVoteEvent> onChainVotes
    );
    string SignSettlementProposal(Guid thingId, Guid proposalId);
    string SignAssessmentPollVoteAgg(
        IEnumerable<AssessmentPollVote> offChainVotes, IEnumerable<CastedAssessmentPollVoteEvent> onChainVotes
    );
    string SignTimestamp(DateTimeOffset timestamp);
}