using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IActionableThingRelatedEventRepository : IRepository<ActionableThingRelatedEvent>
{
    void Create(ActionableThingRelatedEvent @event);
}