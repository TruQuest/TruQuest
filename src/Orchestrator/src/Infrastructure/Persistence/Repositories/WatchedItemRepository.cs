using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class WatchedItemRepository : Repository<WatchedItem>, IWatchedItemRepository
{
    private readonly AppDbContext _dbContext;

    public WatchedItemRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Add(WatchedItem watchedItem)
    {
        _dbContext.WatchList.Add(watchedItem);
    }

    public void Remove(WatchedItem watchedItem)
    {
        _dbContext.WatchList.Remove(watchedItem);
    }
}