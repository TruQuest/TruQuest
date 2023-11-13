using GoThataway;
using FluentValidation;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.User.Commands.MarkNotificationsAsRead;

[RequireAuthorization]
public class MarkNotificationsAsReadCommand : IRequest<VoidResult>
{
    public required IEnumerable<NotificationIm> Notifications { get; init; }
}

internal class Validator : AbstractValidator<MarkNotificationsAsReadCommand>
{
    public Validator()
    {
        RuleFor(c => c.Notifications).NotEmpty();
        RuleForEach(c => c.Notifications).SetValidator(new NotificationImValidator());
    }
}

public class MarkNotificationsAsReadCommandHandler : IRequestHandler<MarkNotificationsAsReadCommand, VoidResult>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public MarkNotificationsAsReadCommandHandler(
        ICurrentPrincipal currentPrincipal,
        IWatchedItemRepository watchedItemRepository
    )
    {
        _currentPrincipal = currentPrincipal;
        _watchedItemRepository = watchedItemRepository;
    }

    public async Task<VoidResult> Handle(MarkNotificationsAsReadCommand command, CancellationToken ct)
    {
        await _watchedItemRepository.UpdateLastSeenTimestamp(command.Notifications.Select(n => new WatchedItem(
            userId: _currentPrincipal.Id!,
            itemType: (WatchedItemType)n.ItemType,
            itemId: n.ItemId,
            itemUpdateCategory: n.ItemUpdateCategory,
            lastSeenUpdateTimestamp: n.UpdateTimestamp
        )));

        return VoidResult.Instance;
    }
}
