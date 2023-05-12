using Application.User.Queries.GetWatchListUpdates;

namespace API.Hubs.Clients;

public interface ITruQuestClient
{
    Task TellAboutNewThingDraftCreationProgress(string thingId, int percent);
    Task NotifyThingStateChanged(string thingId, int state);
    Task TellAboutNewSettlementProposalDraftCreationProgress(string proposalId, int percent);
    Task NotifySettlementProposalStateChanged(string proposalId, int state);
    Task NotifyAboutItemUpdate(
        long updateTimestamp, int itemType, string itemId,
        int itemUpdateCategory, string title, string? details
    );
    Task OnInitialNotificationRetrieve(IEnumerable<WatchedItemUpdateQm> updates);
}