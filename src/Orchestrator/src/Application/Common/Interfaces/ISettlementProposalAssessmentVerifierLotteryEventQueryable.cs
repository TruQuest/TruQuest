using Domain.Aggregates.Events;

namespace Application.Common.Interfaces;

public interface ISettlementProposalAssessmentVerifierLotteryEventQueryable
{
    Task<List<ThingAssessmentVerifierLotterySpotClaimedEvent>> GetAllSpotClaimedEventsFor(
        Guid thingId, Guid settlementProposalId
    );
}