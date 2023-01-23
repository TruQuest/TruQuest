using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IPreJoinedThingAssessmentVerifierLotteryEventRepository : IRepository<PreJoinedThingAssessmentVerifierLotteryEvent>
{
    void Create(PreJoinedThingAssessmentVerifierLotteryEvent @event);
}