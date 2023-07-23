using Domain.Errors;
using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Thing.Commands.CastAcceptancePollVote;
using Application.Settlement.Commands.CastAssessmentPollVote;

namespace Application.Common.Interfaces;

public interface ISigner
{
    string RecoverFromNewAcceptancePollVoteMessage(NewAcceptancePollVoteIm input, string signature);
    string RecoverFromNewAssessmentPollVoteMessage(NewAssessmentPollVoteIm input, string signature);

    string SignThing(Guid thingId);

    string SignNewAcceptancePollVote(
        NewAcceptancePollVoteIm input, string walletAddress, string ownerAddress, string ownerSignature
    );

    string SignNewAssessmentPollVote(
        NewAssessmentPollVoteIm input, string walletAddress, string ownerAddress, string ownerSignature
    );

    string SignAcceptancePollVoteAgg(
        Guid thingId,
        ulong endBlock,
        IEnumerable<AcceptancePollVote> offChainVotes,
        IEnumerable<CastedAcceptancePollVoteEvent> onChainVotes
    );

    string SignSettlementProposal(Guid thingId, Guid proposalId);

    string SignAssessmentPollVoteAgg(
        Guid thingId, Guid proposalId, ulong endBlock,
        IEnumerable<AssessmentPollVote> offChainVotes,
        IEnumerable<CastedAssessmentPollVoteEvent> onChainVotes
    );

    bool CheckIsOrchestrator(String address);

    string RecoverFromSiweMessage(string message, string signature);
}
