using Domain.Aggregates.Events;

using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface ISettlementProposalAssessmentVerifierLotteryEventQueryable
{
    Task<List<ThingAssessmentVerifierLotterySpotClaimedEvent>> FindAllSpotClaimedEventsFor(
        Guid thingId, Guid settlementProposalId
    );

    Task<List<JoinedThingAssessmentVerifierLotteryEvent>> FindJoinedEventsWithClosestNonces(
        Guid thingId, Guid settlementProposalId, long latestBlockNumber, decimal nonce, int count
    );

    Task<IEnumerable<VerifierLotteryWinnerQm>> GetLotteryWinnerIndicesAccordingToPreJoinedEvents(
        Guid thingId, Guid settlementProposalId, IEnumerable<string> winnerIds
    );
}