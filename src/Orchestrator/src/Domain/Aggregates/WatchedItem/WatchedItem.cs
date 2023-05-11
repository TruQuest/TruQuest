using Domain.Base;

namespace Domain.Aggregates;

public class WatchedItem : Entity, IAggregateRoot
{
    public string UserId { get; }
    public WatchedItemType ItemType { get; }
    public Guid ItemId { get; }
    public long LastCheckedAt { get; private set; }

    public WatchedItem(string userId, WatchedItemType itemType, Guid itemId, long lastCheckedAt)
    {
        UserId = userId;
        ItemType = itemType;
        ItemId = itemId;
        LastCheckedAt = lastCheckedAt;
    }

    public void SetLastCheckedAt(long lastCheckedAt)
    {
        LastCheckedAt = lastCheckedAt;
    }
}