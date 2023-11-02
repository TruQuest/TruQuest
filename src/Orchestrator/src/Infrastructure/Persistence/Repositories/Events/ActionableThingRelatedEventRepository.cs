using Domain.Aggregates.Events;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ActionableThingRelatedEventRepository : Repository, IActionableThingRelatedEventRepository
{
    private new readonly EventDbContext _dbContext;

    public ActionableThingRelatedEventRepository(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(ActionableThingRelatedEvent @event)
    {
        _dbContext.ActionableThingRelatedEvents.Add(@event);
    }
}
