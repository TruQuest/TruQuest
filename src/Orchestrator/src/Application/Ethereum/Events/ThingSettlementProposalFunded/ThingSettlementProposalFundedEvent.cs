using MediatR;

using Domain.Aggregates.Events;

using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.ThingSettlementProposalFunded;

public class ThingSettlementProposalFundedEvent : BaseContractEvent, INotification
{
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
            txnHash: @event.TxnHash,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.SettlementProposalFunded
        );

        var payload = new Dictionary<string, object>()
        {
            ["settlementProposalId"] = new Guid(@event.SettlementProposalId),
            ["userId"] = @event.UserId,
            ["stake"] = @event.Stake
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        proposalFundedEvent.SetPayload(payload);

        _actionableThingRelatedEventRepository.Create(proposalFundedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}
