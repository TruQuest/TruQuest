using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Thing.Commands.Watch;

[RequireAuthorization]
public class WatchCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
}

internal class WatchCommandHandler : IRequestHandler<WatchCommand, VoidResult>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public WatchCommandHandler(
        ICurrentPrincipal currentPrincipal,
        IWatchedItemRepository watchedItemRepository
    )
    {
        _currentPrincipal = currentPrincipal;
        _watchedItemRepository = watchedItemRepository;
    }

    public async Task<VoidResult> Handle(WatchCommand command, CancellationToken ct)
    {
        _watchedItemRepository.Add(new WatchedItem(
            userId: _currentPrincipal.Id!,
            itemType: WatchedItemType.Thing,
            itemId: command.ThingId,
            lastCheckedAt: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        ));

        await _watchedItemRepository.SaveChanges();

        return VoidResult.Instance;
    }
}