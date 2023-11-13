using GoThataway;

using Domain.Aggregates.Events;

using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.SettlementProposalFunded;

public class SettlementProposalFundedEvent : BaseContractEvent, IEvent
{
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string WalletAddress { get; init; }
    public required decimal Stake { get; init; }
}

public class SettlementProposalFundedEventHandler : IEventHandler<SettlementProposalFundedEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public SettlementProposalFundedEventHandler(
        IActionableThingRelatedEventRepository actionableThingRelatedEventRepository
    )
    {
        _actionableThingRelatedEventRepository = actionableThingRelatedEventRepository;
    }

    public async Task Handle(SettlementProposalFundedEvent @event, CancellationToken ct)
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
            ["walletAddress"] = @event.WalletAddress,
            ["stake"] = @event.Stake
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        proposalFundedEvent.SetPayload(payload);

        _actionableThingRelatedEventRepository.Create(proposalFundedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}
