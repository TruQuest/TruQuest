using MediatR;

using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.Lottery.VerifierLotteryClosedWithSuccess;

public class VerifierLotteryClosedWithSuccessEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public decimal Nonce { get; init; }
    public required List<string> WinnerIds { get; init; }
}

internal class VerifierLotteryClosedWithSuccessEventHandler : INotificationHandler<VerifierLotteryClosedWithSuccessEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public VerifierLotteryClosedWithSuccessEventHandler(
        IActionableThingRelatedEventRepository actionableThingRelatedEventRepository
    )
    {
        _actionableThingRelatedEventRepository = actionableThingRelatedEventRepository;
    }

    public async Task Handle(VerifierLotteryClosedWithSuccessEvent @event, CancellationToken ct)
    {
        var lotteryClosedEvent = new ActionableThingRelatedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.VerifierLotteryClosedWithSuccess
        );
        lotteryClosedEvent.SetPayload(new()
        {
            ["nonce"] = @event.Nonce,
            ["winnerIds"] = @event.WinnerIds
        });
        _actionableThingRelatedEventRepository.Create(lotteryClosedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}