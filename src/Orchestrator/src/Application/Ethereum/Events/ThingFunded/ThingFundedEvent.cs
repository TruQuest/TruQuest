using MediatR;

using Domain.Aggregates.Events;

using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.ThingFunded;

public class ThingFundedEvent : BaseContractEvent, INotification
{
    public required byte[] ThingId { get; init; }
    public required string UserId { get; init; }
    public required decimal Stake { get; init; }
}

internal class ThingFundedEventHandler : INotificationHandler<ThingFundedEvent>
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
            ["userId"] = @event.UserId,
            ["stake"] = @event.Stake
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        thingFundedEvent.SetPayload(payload);

        _actionableThingRelatedEventRepository.Create(thingFundedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}
