using MediatR;

using Domain.Aggregates.Events;

using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.ThingValidationVerifierLottery.LotteryClosedInFailure;

public class LotteryClosedInFailureEvent : BaseContractEvent, INotification
{
    public required byte[] ThingId { get; init; }
    public required int RequiredNumVerifiers { get; init; }
    public required int JoinedNumVerifiers { get; init; }
}

internal class LotteryClosedInFailureEventHandler : INotificationHandler<LotteryClosedInFailureEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public LotteryClosedInFailureEventHandler(
        IActionableThingRelatedEventRepository actionableThingRelatedEventRepository
    )
    {
        _actionableThingRelatedEventRepository = actionableThingRelatedEventRepository;
    }

    public async Task Handle(LotteryClosedInFailureEvent @event, CancellationToken ct)
    {
        var lotteryClosedEvent = new ActionableThingRelatedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.ValidationVerifierLotteryClosedInFailure
        );

        var payload = new Dictionary<string, object>()
        {
            ["requiredNumVerifiers"] = @event.RequiredNumVerifiers,
            ["joinedNumVerifiers"] = @event.JoinedNumVerifiers
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        lotteryClosedEvent.SetPayload(payload);

        _actionableThingRelatedEventRepository.Create(lotteryClosedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}