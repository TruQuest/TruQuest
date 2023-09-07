using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ActionableThingRelatedEventRepository : Repository, IActionableThingRelatedEventRepository
{
    private new readonly EventDbContext _dbContext;

    public ActionableThingRelatedEventRepository(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(ActionableThingRelatedEvent @event)
    {
        _dbContext.ActionableThingRelatedEvents.Add(@event);
    }
}
