using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class WatchListQueryable : Queryable, IWatchListQueryable
{
    private readonly AppDbContext _dbContext;

    public WatchListQueryable(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<string>> GetWatchersFor(WatchedItemType itemType, Guid itemId)
    {
        var watchers = await _dbContext.WatchList
            .Where(w => w.ItemType == itemType && w.ItemId == itemId)
            .Select(w => w.UserId)
            .ToListAsync();

        return watchers;
    }
}