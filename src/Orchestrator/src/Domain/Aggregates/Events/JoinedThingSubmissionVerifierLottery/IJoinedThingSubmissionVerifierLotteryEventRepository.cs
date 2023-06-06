using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IJoinedThingSubmissionVerifierLotteryEventRepository :
    IRepository<JoinedThingSubmissionVerifierLotteryEvent>
{
    void Create(JoinedThingSubmissionVerifierLotteryEvent @event);
    Task<List<JoinedThingSubmissionVerifierLotteryEvent>> FindAllFor(Guid thingId);
    Task UpdateNoncesFor(IEnumerable<JoinedThingSubmissionVerifierLotteryEvent> events);
}