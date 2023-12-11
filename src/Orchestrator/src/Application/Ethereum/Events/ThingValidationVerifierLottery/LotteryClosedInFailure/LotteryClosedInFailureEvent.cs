using GoThataway;

using Domain.Aggregates.Events;

using Application.Common.Monitoring;
using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.ThingValidationVerifierLottery.LotteryClosedInFailure;

public class LotteryClosedInFailureEvent : BaseContractEvent, IEvent
{
    public required byte[] ThingId { get; init; }
    public required int RequiredNumVerifiers { get; init; }
    public required int JoinedNumVerifiers { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.ThingId, new Guid(ThingId)),
            (ActivityTags.TxnHash, TxnHash)
        };
    }
}

public class LotteryClosedInFailureEventHandler : IEventHandler<LotteryClosedInFailureEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public LotteryClosedInFailureEventHandler(IActionableThingRelatedEventRepository actionableThingRelatedEventRepository)
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
            type: ThingEventType.ValidationVerifierLotteryFailed
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
