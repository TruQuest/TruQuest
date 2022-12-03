using MediatR;

using Domain.Aggregates;

namespace Application.Ethereum.Events.Lottery.VerifierLotteryClosedWithSuccess;

public class VerifierLotteryClosedWithSuccessEvent : INotification
{
    public required string ThingIdHash { get; init; }
    public required List<string> WinnerIds { get; set; }
}

internal class VerifierLotteryClosedWithSuccessEventHandler : INotificationHandler<VerifierLotteryClosedWithSuccessEvent>
{
    private readonly IThingRepository _thingRepository;

    public async Task Handle(VerifierLotteryClosedWithSuccessEvent @event, CancellationToken ct)
    {
        var thing = await _thingRepository.FindByIdHash(@event.ThingIdHash);

    }
}