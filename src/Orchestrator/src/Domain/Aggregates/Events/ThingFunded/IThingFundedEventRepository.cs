using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IThingFundedEventRepository : IRepository<ThingFundedEvent>
{
    void Create(ThingFundedEvent @event);
    void UpdateProcessedStateFor(long id, bool processed);
}