using Microsoft.AspNetCore.SignalR;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace API.Hubs.Clients;

internal class ClientNotifier : IClientNotifier
{
    private readonly ILogger<ClientNotifier> _logger;
    private readonly IHubContext<TruQuestHub, ITruQuestClient> _hubContext;

    public ClientNotifier(
        ILogger<ClientNotifier> logger,
        IHubContext<TruQuestHub, ITruQuestClient> hubContext
    )
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public Task SubscribeToStream(string connectionId, string updateStreamIdentifier)
    {
        return _hubContext.Groups.AddToGroupAsync(connectionId, updateStreamIdentifier);
    }

    public Task UnsubscribeFromStream(string connectionId, string updateStreamIdentifier)
    {
        return _hubContext.Groups.RemoveFromGroupAsync(connectionId, updateStreamIdentifier);
    }

    public Task TellAboutNewThingDraftCreationProgress(string userId, Guid thingId, int percent) =>
        _hubContext.Clients.User(userId).TellAboutNewThingDraftCreationProgress(thingId.ToString(), percent);

    public Task TellAboutNewSettlementProposalDraftCreationProgress(string userId, Guid proposalId, int percent) =>
        _hubContext.Clients.User(userId).TellAboutNewSettlementProposalDraftCreationProgress(
            proposalId.ToString(), percent
        );

    public async Task NotifyUsersAboutItemUpdate(
        IEnumerable<string> userIds, long updateTimestamp, WatchedItemType itemType,
        Guid itemId, int itemUpdateCategory, string title, string? details
    )
    {
        var updateStreamIdentifier = (itemType == WatchedItemType.Subject ? "/subjects" :
            itemType == WatchedItemType.Thing ? "/things" : "/proposals") + $"/{itemId}";

        await _hubContext.Clients.Group(updateStreamIdentifier).NotifyAboutItemUpdate(
            updateTimestamp, (int)itemType, itemId.ToString(), itemUpdateCategory, title, details
        );

        // @@TODO: If user watches an item and also happens to be on the item's page at the moment,
        // he will receive the same update notification twice. Is there some way to deal with it server-side?

        await _hubContext.Clients.Users(userIds).NotifyAboutItemUpdate(
            updateTimestamp, (int)itemType, itemId.ToString(), itemUpdateCategory, title, details
        );
    }

    public async Task NotifyUsersAboutSpecialItemUpdate(
        IEnumerable<string> userIds, long updateTimestamp, WatchedItemType itemType,
        Guid itemId, int itemUpdateCategory, string title, string? details
    )
    {
        await _hubContext.Clients.Users(userIds).NotifyAboutItemUpdate(
            updateTimestamp, (int)itemType, itemId.ToString(), itemUpdateCategory, title, details
        );
    }
}