using Microsoft.AspNetCore.SignalR;

using GoThataway;

using Domain.Results;
using Application.User.Queries.GetWatchListUpdates;
using Application.User.Commands.SubscribeToUpdates;
using Application.User.Commands.UnsubscribeFromUpdates;
using Application.User.Commands.UnsubThenSubToUpdates;
using static Application.Common.Monitoring.LogMessagePlaceholders;

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
        if (Context.UserIdentifier != null)
        {
            var result = await _thataway.Send(new GetWatchListUpdatesQuery { UserId = Context.UserIdentifier });
            _logger.LogDebug(
                $"Retrieved {result.Data!.Count()} notifications for user {UserId}",
                Context.UserIdentifier
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
