using Domain.Base;

namespace Domain.Aggregates;

public interface IWatchedItemRepository : IRepository<WatchedItem>
{
    void Add(params WatchedItem[] watchedItems);
    void Remove(WatchedItem watchedItem);
    Task UpdateLastSeenTimestamp(IEnumerable<WatchedItem> watchedItems);
}