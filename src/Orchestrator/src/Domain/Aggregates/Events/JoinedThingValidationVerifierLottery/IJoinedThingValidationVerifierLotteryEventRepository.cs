using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IJoinedThingValidationVerifierLotteryEventRepository :
    IRepository<JoinedThingValidationVerifierLotteryEvent>
{
    void Create(JoinedThingValidationVerifierLotteryEvent @event);
    Task<List<JoinedThingValidationVerifierLotteryEvent>> FindAllFor(Guid thingId);
    Task UpdateNoncesFor(IEnumerable<JoinedThingValidationVerifierLotteryEvent> events);
}
