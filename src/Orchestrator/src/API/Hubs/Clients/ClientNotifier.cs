using Microsoft.AspNetCore.SignalR;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace API.Hubs.Clients;

internal class ClientNotifier : IClientNotifier
{
    private readonly IHubContext<TruQuestHub, ITruQuestClient> _hubContext;

    public ClientNotifier(IHubContext<TruQuestHub, ITruQuestClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task TellAboutNewThingDraftCreationProgress(string userId, Guid thingId, int percent) =>
        _hubContext.Clients.User(userId).TellAboutNewThingDraftCreationProgress(thingId.ToString(), percent);

    public Task TellThatNewThingDraftCreatedSuccessfully(string userId, Guid thingId)
    {
        return Task.CompletedTask;
    }

    public Task SubscribeToThing(string connectionId, Guid thingId)
    {
        return _hubContext.Groups.AddToGroupAsync(connectionId, $"thing:{thingId}");
    }

    public Task UnsubscribeFromThing(string connectionId, Guid thingId)
    {
        return _hubContext.Groups.RemoveFromGroupAsync(connectionId, $"thing:{thingId}");
    }

    public Task NotifyThingStateChanged(Guid thingId, ThingState state)
    {
        return _hubContext.Clients.Group($"thing:{thingId}").NotifyThingStateChanged(thingId.ToString(), (int)state);
    }

    public Task SubscribeToSettlementProposal(string connectionId, Guid proposalId)
    {
        return _hubContext.Groups.AddToGroupAsync(connectionId, $"proposal:{proposalId}");
    }

    public Task UnsubscribeFromSettlementProposal(string connectionId, Guid proposalId)
    {
        return _hubContext.Groups.RemoveFromGroupAsync(connectionId, $"proposal:{proposalId}");
    }

    public Task TellAboutNewSettlementProposalDraftCreationProgress(string userId, Guid proposalId, int percent) =>
        _hubContext.Clients.User(userId).TellAboutNewSettlementProposalDraftCreationProgress(
            proposalId.ToString(), percent
        );

    public Task NotifySettlementProposalStateChanged(Guid proposalId, SettlementProposalState state)
    {
        return _hubContext.Clients
            .Group($"proposal:{proposalId}")
            .NotifySettlementProposalStateChanged(proposalId.ToString(), (int)state);
    }

    public Task NotifyUsersAboutItemUpdate(
        IEnumerable<string> userIds, long updateTimestamp, WatchedItemType itemType,
        Guid itemId, int itemUpdateCategory, string title, string? details
    ) => _hubContext.Clients.Users(userIds).NotifyAboutItemUpdate(
        updateTimestamp, (int)itemType, itemId.ToString(), itemUpdateCategory, title, details
    );
}