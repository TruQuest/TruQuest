using Microsoft.AspNetCore.SignalR;

using GoThataway;

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
    private readonly Thataway _thataway;

    public TruQuestHub(ILogger<TruQuestHub> logger, Thataway thataway)
    {
        _logger = logger;
        _thataway = thataway;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _logger.LogInformation($"{Context.UserIdentifier ?? "Guest"} connected!");

        if (Context.UserIdentifier != null)
        {
            var result = await _thataway.Send(new GetWatchListUpdatesQuery { UserId = Context.UserIdentifier });
            _logger.LogInformation(
                "User {UserId}: Retrieved {Count} notifications",
                Context.UserIdentifier,
                result.Data!.Count()
            );

            await Clients.Caller.OnInitialNotificationRetrieve(result.Data!);
        }
    }

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> SubscribeToUpdates(SubscribeToUpdatesCommand command) =>
        _thataway.Send(command);

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> UnsubscribeFromUpdates(UnsubscribeFromUpdatesCommand command) =>
        _thataway.Send(command);

    [AddConnectionIdProviderToMethodInvocationScope]
    public Task<VoidResult> UnsubThenSubToUpdates(UnsubThenSubToUpdatesCommand command) =>
        _thataway.Send(command);
}
