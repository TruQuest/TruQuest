using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;

using Application.Common.Interfaces;

namespace Application.User.Commands.NotifyWatchers;

public class NotifyWatchersCommand : IRequest<VoidResult>
{
    public required WatchedItemTypeIm ItemType { get; init; }
    public required Guid ItemId { get; init; }
    public required long UpdateTimestamp { get; init; }
    public required string Title { get; init; }
    public required string? Details { get; init; }
}

internal class NotifyWatchersCommandHandler : IRequestHandler<NotifyWatchersCommand, VoidResult>
{
    private readonly ILogger<NotifyWatchersCommandHandler> _logger;
    private readonly IWatchListQueryable _watchListQueryable;
    private readonly IClientNotifier _clientNotifier;

    public NotifyWatchersCommandHandler(
        ILogger<NotifyWatchersCommandHandler> logger,
        IWatchListQueryable watchListQueryable,
        IClientNotifier clientNotifier
    )
    {
        _logger = logger;
        _watchListQueryable = watchListQueryable;
        _clientNotifier = clientNotifier;
    }

    public async Task<VoidResult> Handle(NotifyWatchersCommand command, CancellationToken ct)
    {
        var watcherIds = await _watchListQueryable.GetWatchersFor(
            (WatchedItemType)command.ItemType, command.ItemId
        );
        if (watcherIds.Any())
        {
            await _clientNotifier.NotifyUsersAboutItemUpdate(
                watcherIds, command.UpdateTimestamp, (WatchedItemType)command.ItemType,
                command.ItemId, command.Title, command.Details
            );
        }

        return VoidResult.Instance;
    }
}