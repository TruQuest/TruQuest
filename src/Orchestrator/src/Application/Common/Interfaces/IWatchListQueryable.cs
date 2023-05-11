using Domain.Aggregates;

namespace Application.Common.Interfaces;

public interface IWatchListQueryable
{
    Task<IEnumerable<string>> GetWatchersFor(WatchedItemType itemType, Guid itemId);
}