using Domain.Aggregates;

namespace Application.Common.Interfaces;

public interface IClientNotifier
{
    Task SubscribeToStream(string connectionId, string updateStreamIdentifier);
    Task UnsubscribeFromStream(string connectionId, string updateStreamIdentifier);
    Task TellAboutNewThingDraftCreationProgress(string userId, Guid thingId, int percent);
    Task TellAboutNewSettlementProposalDraftCreationProgress(string userId, Guid proposalId, int percent);
    Task NotifyUsersAboutItemUpdate(
        IEnumerable<string> userIds, long updateTimestamp, WatchedItemType itemType,
        Guid itemId, int itemUpdateCategory, string title, string? details
    );
}