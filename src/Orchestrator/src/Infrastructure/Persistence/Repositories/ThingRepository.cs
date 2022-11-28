using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class ThingRepository : Repository<Thing>, IThingRepository
{
    private readonly AppDbContext _dbContext;

    public ThingRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(Thing thing)
    {
        _dbContext.Things.Add(thing);
    }
}