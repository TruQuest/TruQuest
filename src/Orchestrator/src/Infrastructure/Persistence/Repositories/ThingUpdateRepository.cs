using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class ThingUpdateRepository : Repository<ThingUpdate>, IThingUpdateRepository
{
    private readonly AppDbContext _dbContext;

    public ThingUpdateRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Add(ThingUpdate updateEvent)
    {
        // @@TODO: Handle possible primary key constraint violation.
        _dbContext.ThingUpdates.Add(updateEvent);
    }
}