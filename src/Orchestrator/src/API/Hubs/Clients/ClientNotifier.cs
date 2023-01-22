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
}