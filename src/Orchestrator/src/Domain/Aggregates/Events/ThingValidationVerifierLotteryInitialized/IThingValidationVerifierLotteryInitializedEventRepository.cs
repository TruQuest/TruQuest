using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IThingValidationVerifierLotteryInitializedEventRepository :
    IRepository<ThingValidationVerifierLotteryInitializedEvent>
{
    void Create(ThingValidationVerifierLotteryInitializedEvent @event);
}
