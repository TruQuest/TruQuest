using MediatR;

using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.ThingSubmissionVerifierLottery.LotteryClosedInFailure;

public class LotteryClosedInFailureEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
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
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.SubmissionVerifierLotteryClosedInFailure
        );
        lotteryClosedEvent.SetPayload(new()
        {
            ["requiredNumVerifiers"] = @event.RequiredNumVerifiers,
            ["joinedNumVerifiers"] = @event.JoinedNumVerifiers
        });
        _actionableThingRelatedEventRepository.Create(lotteryClosedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}