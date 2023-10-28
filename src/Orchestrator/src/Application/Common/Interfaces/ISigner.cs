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
        NewAcceptancePollVoteIm input, string userId, string walletAddress, string signerAddress, string signature
    );

    string SignNewAssessmentPollVote(
        NewAssessmentPollVoteIm input, string userId, string walletAddress, string signerAddress, string signature
    );

    string SignAcceptancePollVoteAgg(
        Guid thingId,
        ulong l1EndBlock,
        IEnumerable<AcceptancePollVote> offChainVotes,
        IEnumerable<CastedAcceptancePollVoteEvent> onChainVotes
    );

    string SignSettlementProposal(Guid thingId, Guid proposalId);

    string SignAssessmentPollVoteAgg(
        Guid thingId, Guid proposalId, ulong l1EndBlock,
        IEnumerable<AssessmentPollVote> offChainVotes,
        IEnumerable<CastedAssessmentPollVoteEvent> onChainVotes
    );

    string RecoverFromSiweMessage(string message, string signature); // @@TODO: Rename this.
}
