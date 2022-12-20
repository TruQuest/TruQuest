using MediatR;

using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.ThingSettlementProposalFunded;

public class ThingSettlementProposalFundedEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required string ThingIdHash { get; init; }
    public required string SettlementProposalIdHash { get; init; }
    public required string UserId { get; init; }
    public decimal Stake { get; init; }
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
            thingIdHash: @event.ThingIdHash,
            type: ThingEventType.ThingSettlementProposalFunded
        );
        proposalFundedEvent.SetPayload(new()
        {
            ["settlementProposalIdHash"] = @event.SettlementProposalIdHash,
            ["userId"] = @event.UserId,
            ["stake"] = @event.Stake
        });
        _actionableThingRelatedEventRepository.Create(proposalFundedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}