using Domain.Aggregates;

using Application.User.Queries.GetWatchListUpdates;

namespace Application.Common.Interfaces;

public interface IWatchListQueryable
{
    Task<IEnumerable<string>> GetWatchersFor(WatchedItemType itemType, Guid itemId, int itemUpdateCategory);
    Task<IEnumerable<WatchedItemUpdateQm>> GetLatestUpdatesFor(string userId);
}