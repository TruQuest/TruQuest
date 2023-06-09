using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IJoinedThingAssessmentVerifierLotteryEventRepository : IRepository<JoinedThingAssessmentVerifierLotteryEvent>
{
    void Create(JoinedThingAssessmentVerifierLotteryEvent @event);
    Task<List<JoinedThingAssessmentVerifierLotteryEvent>> FindAllFor(Guid thingId, Guid proposalId);
    Task UpdateNoncesFor(IEnumerable<JoinedThingAssessmentVerifierLotteryEvent> events);
}