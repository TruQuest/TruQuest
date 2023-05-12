using Domain.Base;

namespace Domain.Aggregates;

public interface IWatchedItemRepository : IRepository<WatchedItem>
{
    void Add(WatchedItem watchedItem);
    void Remove(WatchedItem watchedItem);
    Task UpdateLastSeenTimestamp(IEnumerable<WatchedItem> watchedItems);
}