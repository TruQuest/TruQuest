using Domain.Base;

namespace Domain.Aggregates;

public class WatchedItem : Entity, IAggregateRoot
{
    public string UserId { get; }
    public WatchedItemType ItemType { get; }
    public Guid ItemId { get; }
    public int ItemUpdateCategory { get; }
    public long LastSeenUpdateTimestamp { get; private set; }

    public WatchedItem(
        string userId, WatchedItemType itemType, Guid itemId,
        int itemUpdateCategory, long lastSeenUpdateTimestamp = -1
    )
    {
        UserId = userId;
        ItemType = itemType;
        ItemId = itemId;
        ItemUpdateCategory = itemUpdateCategory;
        LastSeenUpdateTimestamp = lastSeenUpdateTimestamp;
    }

    public void SetLastSeenUpdateTimestamp(long lastSeenUpdateTimestamp)
    {
        LastSeenUpdateTimestamp = lastSeenUpdateTimestamp;
    }
}