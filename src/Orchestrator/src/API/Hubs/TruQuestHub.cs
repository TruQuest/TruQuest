using Microsoft.AspNetCore.SignalR;

using Domain.Results;
using Application.User.Queries.GetWatchListUpdates;
using Application.User.Commands.SubscribeToUpdates;
using Application.User.Commands.UnsubscribeFromUpdates;
using Application.User.Commands.UnsubThenSubToUpdates;
using Application.Common.Interfaces;

using API.Hubs.Clients;

namespace API.Hubs;

public class TruQuestHub : Hub<ITruQuestClient>
{
    private readonly ILogger<TruQuestHub> _logger;
    private readonly ISenderWrapper _sender;

    public TruQuestHub(ILogger<TruQuestHub> logger, ISenderWrapper sender)
    {
        _logger = logger;
        _sender = sender;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _logger.LogInformation($"{Context.UserIdentifier ?? "Guest"} connected!");

        if (Context.UserIdentifier != null)
        {
            var result = await _sender.Send(
                new GetWatchListUpdatesQuery
                {
                    UserId = Context.UserIdentifier
                },
                serviceProvider: Context.GetHttpContext()!.RequestServices
            );

            _logger.LogInformation(
                "User {UserId}: Retrieved {Count} notifications",
                Context.UserIdentifier,
                result.Data!.Count()
            );

            await Clients.Caller.OnInitialNotificationRetrieve(result.Data!);
        }
    }

    public Task<VoidResult> SubscribeToUpdates(SubscribeToUpdatesCommand command) => _sender.Send(
        command,
        serviceProvider: Context.GetHttpContext()!.RequestServices,
        signalRConnectionId: Context.ConnectionId
    );

    public Task<VoidResult> UnsubscribeFromUpdates(UnsubscribeFromUpdatesCommand command) => _sender.Send(
        command,
        serviceProvider: Context.GetHttpContext()!.RequestServices,
        signalRConnectionId: Context.ConnectionId
    );

    public Task<VoidResult> UnsubThenSubToUpdates(UnsubThenSubToUpdatesCommand command) => _sender.Send(
        command,
        serviceProvider: Context.GetHttpContext()!.RequestServices,
        signalRConnectionId: Context.ConnectionId
    );
}
