using MediatR;

using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.ThingSettlementProposalFunded;

public class ThingSettlementProposalFundedEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string UserId { get; init; }
    public required decimal Stake { get; init; }
}

internal class ThingSettlementProposalFundedEventHandler : INotificationHandler<ThingSettlementProposalFundedEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public ThingSettlementProposalFundedEventHandler(
        IActionableThingRelatedEventRepository actionableThingRelatedEventRepository
    )
    {
        _actionableThingRelatedEventRepository = actionableThingRelatedEventRepository;
    }

    public async Task Handle(ThingSettlementProposalFundedEvent @event, CancellationToken ct)
    {
        var proposalFundedEvent = new ActionableThingRelatedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.SettlementProposalFunded
        );
        proposalFundedEvent.SetPayload(new()
        {
            ["settlementProposalId"] = new Guid(@event.SettlementProposalId),
            ["userId"] = @event.UserId,
            ["stake"] = @event.Stake
        });
        _actionableThingRelatedEventRepository.Create(proposalFundedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}