using Microsoft.AspNetCore.SignalR;

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
}