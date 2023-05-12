using Application.User.Common.Models.IM;

namespace Application.User.Commands.MarkNotificationsAsRead;

public class NotificationIm
{
    public required WatchedItemTypeIm ItemType { get; init; }
    public required Guid ItemId { get; init; }
    public required int ItemUpdateCategory { get; init; }
    public required long UpdateTimestamp { get; init; }
}