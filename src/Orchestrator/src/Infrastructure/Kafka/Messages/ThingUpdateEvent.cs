using KafkaFlow;
using KafkaFlow.TypedHandler;

using Domain.Aggregates;
using Application.User.Commands.NotifyWatchers;
using Application.User.Common.Models.IM;

namespace Infrastructure.Kafka.Messages;

internal class ThingUpdateEvent
{
    public required Guid ThingId { get; init; }
    public required ThingUpdateCategory Category { get; init; }
    public required long UpdateTimestamp { get; init; }
    public required string Title { get; init; }
    public required string? Details { get; init; }
}

internal class ThingUpdateEventHandler : IMessageHandler<ThingUpdateEvent>
{
    private readonly SenderWrapper _sender;

    public ThingUpdateEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, ThingUpdateEvent message) =>
        _sender.Send(
            new NotifyWatchersCommand
            {
                ItemType = WatchedItemTypeIm.Thing,
                ItemId = message.ThingId,
                ItemUpdateCategory = (int)message.Category,
                UpdateTimestamp = message.UpdateTimestamp,
                Title = message.Title,
                Details = message.Details
            },
            addToAdditionalSinks: true
        );
}
