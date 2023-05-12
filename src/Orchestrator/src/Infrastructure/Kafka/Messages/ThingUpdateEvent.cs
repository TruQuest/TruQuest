using KafkaFlow;
using KafkaFlow.TypedHandler;
using MediatR;

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
    private readonly ISender _mediator;

    public ThingUpdateEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, ThingUpdateEvent message) =>
        _mediator.Send(new NotifyWatchersCommand
        {
            ItemType = WatchedItemTypeIm.Thing,
            ItemId = message.ThingId,
            ItemUpdateCategory = (int)message.Category,
            UpdateTimestamp = message.UpdateTimestamp,
            Title = message.Title,
            Details = message.Details
        });
}