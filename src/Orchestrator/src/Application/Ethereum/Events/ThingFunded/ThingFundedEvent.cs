using MediatR;

using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.ThingFunded;

public class ThingFundedEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required string ThingIdHash { get; init; }
    public required string UserId { get; init; }
    public decimal Stake { get; init; }
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
            thingIdHash: @event.ThingIdHash,
            type: ThingEventType.ThingFunded
        );
        thingFundedEvent.SetPayload(new()
        {
            ["userId"] = @event.UserId,
            ["stake"] = @event.Stake
        });
        _actionableThingRelatedEventRepository.Create(thingFundedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}