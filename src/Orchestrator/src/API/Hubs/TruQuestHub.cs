using Microsoft.AspNetCore.SignalR;

using MediatR;

using Domain.Results;
using ThingCommands = Application.Thing.Commands;
using SettlementCommands = Application.Settlement.Commands;
using Application.User.Queries.GetWatchListUpdates;

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
    public Task<VoidResult> SubscribeToThingUpdates(
        ThingCommands.SubscribeToUpdates.SubscribeToUpdatesCommand command
    ) => _mediator.Send(command);

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> UnsubscribeFromThingUpdates(
        ThingCommands.UnsubscribeFromUpdates.UnsubscribeFromUpdatesCommand command
    ) => _mediator.Send(command);

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> SubscribeToProposalUpdates(
        SettlementCommands.SubscribeToUpdates.SubscribeToUpdatesCommand command
    ) => _mediator.Send(command);

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> UnsubscribeFromProposalUpdates(
        SettlementCommands.UnsubscribeFromUpdates.UnsubscribeFromUpdatesCommand command
    ) => _mediator.Send(command);
}