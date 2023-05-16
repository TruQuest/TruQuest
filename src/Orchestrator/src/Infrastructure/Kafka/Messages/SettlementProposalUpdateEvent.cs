using KafkaFlow;
using KafkaFlow.TypedHandler;
using MediatR;

using Domain.Aggregates;
using Application.User.Commands.NotifyWatchers;
using Application.User.Common.Models.IM;

namespace Infrastructure.Kafka.Messages;

internal class SettlementProposalUpdateEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required SettlementProposalUpdateCategory Category { get; init; }
    public required long UpdateTimestamp { get; init; }
    public required string Title { get; init; }
    public required string? Details { get; init; }
}

internal class SettlementProposalUpdateEventHandler : IMessageHandler<SettlementProposalUpdateEvent>
{
    private readonly ISender _mediator;

    public SettlementProposalUpdateEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, SettlementProposalUpdateEvent message) =>
        _mediator.Send(new NotifyWatchersCommand
        {
            ItemType = WatchedItemTypeIm.SettlementProposal,
            ItemId = message.SettlementProposalId,
            ItemUpdateCategory = (int)message.Category,
            UpdateTimestamp = message.UpdateTimestamp,
            Title = message.Title,
            Details = message.Details
        });
}