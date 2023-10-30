using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Thing.Commands.CastValidationPollVote;
using Application.Settlement.Commands.CastAssessmentPollVote;

namespace Application.Common.Interfaces;

public interface ISigner
{
    string RecoverFromNewThingValidationPollVoteMessage(NewThingValidationPollVoteIm input, string signature);
    string RecoverFromNewSettlementProposalAssessmentPollVoteMessage(NewSettlementProposalAssessmentPollVoteIm input, string signature);

    string SignThing(Guid thingId);

    string SignNewThingValidationPollVote(
        NewThingValidationPollVoteIm input, string userId, string walletAddress, string signerAddress, string signature
    );

    string SignNewSettlementProposalAssessmentPollVote(
        NewSettlementProposalAssessmentPollVoteIm input, string userId, string walletAddress, string signerAddress, string signature
    );

    string SignThingValidationPollVoteAgg(
        Guid thingId,
        ulong l1EndBlock,
        IEnumerable<ThingValidationPollVote> offChainVotes,
        IEnumerable<CastedThingValidationPollVoteEvent> onChainVotes
    );

    string SignSettlementProposal(Guid thingId, Guid proposalId);

    string SignSettlementProposalAssessmentPollVoteAgg(
        Guid thingId, Guid proposalId, ulong l1EndBlock,
        IEnumerable<SettlementProposalAssessmentPollVote> offChainVotes,
        IEnumerable<CastedSettlementProposalAssessmentPollVoteEvent> onChainVotes
    );

    string RecoverFromMessage(string message, string signature);
}
