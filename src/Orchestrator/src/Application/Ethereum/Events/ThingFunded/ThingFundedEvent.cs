using GoThataway;

using Domain.Aggregates.Events;

using Application.Common.Monitoring;
using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.ThingFunded;

public class ThingFundedEvent : BaseContractEvent, IEvent
{
    public required byte[] ThingId { get; init; }
    public required string WalletAddress { get; init; }
    public required decimal Stake { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.ThingId, new Guid(ThingId)),
            (ActivityTags.WalletAddress, WalletAddress),
            (ActivityTags.TxnHash, TxnHash)
        };
    }
}

public class ThingFundedEventHandler : IEventHandler<ThingFundedEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public ThingFundedEventHandler(IActionableThingRelatedEventRepository actionableThingRelatedEventRepository)
    {
        _actionableThingRelatedEventRepository = actionableThingRelatedEventRepository;
    }

    public async Task Handle(ThingFundedEvent @event, CancellationToken ct)
    {
        var thingFundedEvent = new ActionableThingRelatedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.Funded
        );

        var payload = new Dictionary<string, object>()
        {
            ["walletAddress"] = @event.WalletAddress,
            ["stake"] = @event.Stake
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        thingFundedEvent.SetPayload(payload);

        _actionableThingRelatedEventRepository.Create(thingFundedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}
