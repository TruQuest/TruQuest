using Domain.Aggregates;

namespace Application.User.Queries.GetWatchListUpdates;

public class WatchedItemUpdateQm
{
    public WatchedItemType ItemType { get; }
    public Guid ItemId { get; }
    public int ItemUpdateCategory { get; }
    public long UpdateTimestamp { get; }
    public string Title { get; }
    public string? Details { get; }
}