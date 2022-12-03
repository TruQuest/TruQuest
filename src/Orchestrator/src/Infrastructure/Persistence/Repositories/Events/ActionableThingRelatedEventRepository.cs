using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ActionableThingRelatedEventRepository : Repository<ActionableThingRelatedEvent>, IActionableThingRelatedEventRepository
{
    private readonly EventDbContext _dbContext;

    public ActionableThingRelatedEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(ActionableThingRelatedEvent @event)
    {
        _dbContext.ActionableThingRelatedEvents.Add(@event);
    }
}