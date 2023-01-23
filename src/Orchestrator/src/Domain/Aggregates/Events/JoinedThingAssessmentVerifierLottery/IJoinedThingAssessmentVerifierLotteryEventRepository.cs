using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IJoinedThingAssessmentVerifierLotteryEventRepository : IRepository<JoinedThingAssessmentVerifierLotteryEvent>
{
    void Create(JoinedThingAssessmentVerifierLotteryEvent @event);
}