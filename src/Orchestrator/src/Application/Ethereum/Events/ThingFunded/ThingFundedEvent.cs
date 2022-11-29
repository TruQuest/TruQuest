using MediatR;

using Domain.Aggregates.Events;
using ThingFundedEventDm = Domain.Aggregates.Events.ThingFundedEvent;

namespace Application.Ethereum.Events.ThingFunded;

public class ThingFundedEvent : INotification
{
    public long BlockNumber { get; init; }
    public required string ThingIdHash { get; init; }
    public required string UserId { get; init; }
    public decimal Stake { get; init; }
}

internal class ThingFundedEventHandler : INotificationHandler<ThingFundedEvent>
{
    private readonly IThingFundedEventRepository _thingFundedEventRepository;

    public ThingFundedEventHandler(IThingFundedEventRepository thingFundedEventRepository)
    {
        _thingFundedEventRepository = thingFundedEventRepository;
    }

    public async Task Handle(ThingFundedEvent @event, CancellationToken ct)
    {
        var thingFundedEvent = new ThingFundedEventDm(
            blockNumber: @event.BlockNumber,
            thingIdHash: @event.ThingIdHash,
            userId: @event.UserId,
            stake: @event.Stake
        );
        _thingFundedEventRepository.Create(thingFundedEvent);

        await _thingFundedEventRepository.SaveChanges();
    }
}