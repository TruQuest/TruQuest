using FluentValidation;

using Domain.Aggregates;

using Application.User.Common.Models.IM;

namespace Application.User.Commands.MarkNotificationsAsRead;

public class NotificationIm
{
    public required WatchedItemTypeIm ItemType { get; init; }
    public required Guid ItemId { get; init; }
    public required int ItemUpdateCategory { get; init; }
    public required long UpdateTimestamp { get; init; }
}

internal class NotificationImValidator : AbstractValidator<NotificationIm>
{
    public NotificationImValidator()
    {
        RuleFor(n => n.ItemType).IsInEnum();
        RuleFor(n => n.ItemId).NotEmpty();
        RuleFor(n => n.ItemUpdateCategory).Must(_beAValidUpdateCategory);
        RuleFor(n => n.UpdateTimestamp).GreaterThan(0);
    }

    private bool _beAValidUpdateCategory(int itemUpdateCategory)
    {
        return
            Enum.IsDefined(typeof(SubjectUpdateCategory), itemUpdateCategory) ||
            Enum.IsDefined(typeof(ThingUpdateCategory), itemUpdateCategory) ||
            Enum.IsDefined(typeof(SettlementProposalUpdateCategory), itemUpdateCategory);
    }
}