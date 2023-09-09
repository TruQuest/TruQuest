using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IThingAssessmentVerifierLotteryInitializedEventRepository : IRepository<ThingAssessmentVerifierLotteryInitializedEvent>
{
    void Create(ThingAssessmentVerifierLotteryInitializedEvent @event);
}
