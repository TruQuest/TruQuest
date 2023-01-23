using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IThingAssessmentVerifierLotterySpotClaimedEventRepository : IRepository<ThingAssessmentVerifierLotterySpotClaimedEvent>
{
    void Create(ThingAssessmentVerifierLotterySpotClaimedEvent @event);
}