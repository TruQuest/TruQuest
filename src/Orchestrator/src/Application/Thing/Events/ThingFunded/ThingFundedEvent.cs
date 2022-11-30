using MediatR;

using Domain.Aggregates.Events;
using Domain.Aggregates;

using Application.Common.Attributes;

namespace Application.Thing.Events.ThingFunded;

[ExecuteInTxn]
public class ThingFundedEvent : INotification
{
    public long Id { get; init; }
    public long BlockNumber { get; init; }
    public required string ThingIdHash { get; init; }
}

internal class ThingFundedEventHandler : INotificationHandler<ThingFundedEvent>
{
    private readonly IThingFundedEventRepository _thingFundedEventRepository;
    private readonly IThingRepository _thingRepository;

    public ThingFundedEventHandler(
        IThingFundedEventRepository thingFundedEventRepository,
        IThingRepository thingRepository
    )
    {
        _thingFundedEventRepository = thingFundedEventRepository;
        _thingRepository = thingRepository;
    }

    public async Task Handle(ThingFundedEvent @event, CancellationToken ct)
    {
        _thingFundedEventRepository.UpdateProcessedStateFor(@event.Id, processed: true);
        var thing = await _thingRepository.FindByIdHash(@event.ThingIdHash);
        thing.SetState(ThingState.Funded);
        thing.SetLastUpdatedAt(@event.BlockNumber);

        await _thingFundedEventRepository.SaveChanges();
        await _thingRepository.SaveChanges();
    }
}