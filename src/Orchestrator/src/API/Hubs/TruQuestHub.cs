using Microsoft.AspNetCore.SignalR;

using MediatR;

using Domain.Results;
using Application.Thing.Commands.SubscribeToUpdates;
using Application.Thing.Commands.UnsubscribeFromUpdates;

using API.Hubs.Clients;
using API.Hubs.Filters;

namespace API.Hubs;

public class TruQuestHub : Hub<ITruQuestClient>
{
    private readonly ILogger<TruQuestHub> _logger;
    private readonly ISender _mediator;

    public TruQuestHub(ILogger<TruQuestHub> logger, ISender mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation($"{Context.UserIdentifier ?? "Guest"} connected!");
        return base.OnConnectedAsync();
    }

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> SubscribeToThingUpdates(SubscribeToUpdatesCommand command) => _mediator.Send(command);

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> UnsubscribeFromThingUpdates(UnsubscribeFromUpdatesCommand command) =>
        _mediator.Send(command);
}