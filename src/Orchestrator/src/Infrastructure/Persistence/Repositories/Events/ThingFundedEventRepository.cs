using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ThingFundedEventRepository : Repository<ThingFundedEvent>, IThingFundedEventRepository
{
    private readonly EventDbContext _dbContext;

    public ThingFundedEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(ThingFundedEvent @event)
    {
        _dbContext.ThingFundedEvents.Add(@event);
    }
}