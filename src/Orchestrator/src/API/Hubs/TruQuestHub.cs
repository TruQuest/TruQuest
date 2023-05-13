using Microsoft.AspNetCore.SignalR;

using MediatR;

using Domain.Results;
using Application.User.Queries.GetWatchListUpdates;
using Application.User.Commands.SubscribeToUpdates;
using Application.User.Commands.UnsubscribeFromUpdates;
using Application.User.Commands.UnsubThenSubToUpdates;

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

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _logger.LogInformation($"{Context.UserIdentifier ?? "Guest"} connected!");

        if (Context.UserIdentifier != null)
        {
            var result = await _mediator.Send(new GetWatchListUpdatesQuery
            {
                UserId = Context.UserIdentifier
            });

            _logger.LogInformation(
                "User {UserId}: Retrieved {Count} notifications",
                Context.UserIdentifier,
                result.Data!.Count()
            );

            await Clients.Caller.OnInitialNotificationRetrieve(result.Data!);
        }
    }

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> SubscribeToUpdates(SubscribeToUpdatesCommand command) => _mediator.Send(command);

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> UnsubscribeFromUpdates(UnsubscribeFromUpdatesCommand command) => _mediator.Send(command);

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> UnsubThenSubToUpdates(UnsubThenSubToUpdatesCommand command) => _mediator.Send(command);
}