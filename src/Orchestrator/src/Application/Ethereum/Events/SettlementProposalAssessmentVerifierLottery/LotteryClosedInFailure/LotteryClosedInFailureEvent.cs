using GoThataway;

using Domain.Aggregates.Events;

using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.SettlementProposalAssessmentVerifierLottery.LotteryClosedInFailure;

public class LotteryClosedInFailureEvent : BaseContractEvent, IEvent
{
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required int RequiredNumVerifiers { get; init; }
    public required int JoinedNumVerifiers { get; init; }
}

public class LotteryClosedInFailureEventHandler : IEventHandler<LotteryClosedInFailureEvent>
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
            type: ThingEventType.SettlementProposalAssessmentVerifierLotteryFailed
        );

        var payload = new Dictionary<string, object>()
        {
            ["settlementProposalId"] = new Guid(@event.SettlementProposalId),
            ["requiredNumVerifiers"] = @event.RequiredNumVerifiers,
            ["joinedNumVerifiers"] = @event.JoinedNumVerifiers
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        lotteryClosedEvent.SetPayload(payload);

        _actionableThingRelatedEventRepository.Create(lotteryClosedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}
