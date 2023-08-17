using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IThingAssessmentVerifierLotterySpotClaimedEventRepository : IRepository<ThingAssessmentVerifierLotterySpotClaimedEvent>
{
    void Create(ThingAssessmentVerifierLotterySpotClaimedEvent @event);
    Task<List<ThingAssessmentVerifierLotterySpotClaimedEvent>> FindAllFor(
        Guid thingId, Guid settlementProposalId
    );
    Task UpdateUserDataAndNoncesFor(IEnumerable<ThingAssessmentVerifierLotterySpotClaimedEvent> events);
}
