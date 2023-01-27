using MediatR;

using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.ThingAssessmentVerifierLottery.LotteryClosedWithSuccess;

public class LotteryClosedWithSuccessEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string Orchestrator { get; init; }
    public required decimal Nonce { get; init; }
    public required List<string> ClaimantIds { get; init; }
    public required List<string> WinnerIds { get; init; }
}

internal class LotteryClosedWithSuccessEventHandler : INotificationHandler<LotteryClosedWithSuccessEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public LotteryClosedWithSuccessEventHandler(
        IActionableThingRelatedEventRepository actionableThingRelatedEventRepository
    )
    {
        _actionableThingRelatedEventRepository = actionableThingRelatedEventRepository;
    }

    public async Task Handle(LotteryClosedWithSuccessEvent @event, CancellationToken ct)
    {
        var lotteryClosedEvent = new ActionableThingRelatedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.SettlementProposalAssessmentVerifierLotteryClosedWithSuccess
        );
        lotteryClosedEvent.SetPayload(new()
        {
            ["settlementProposalId"] = new Guid(@event.SettlementProposalId),
            ["orchestrator"] = @event.Orchestrator,
            ["nonce"] = @event.Nonce,
            ["claimantIds"] = @event.ClaimantIds,
            ["winnerIds"] = @event.WinnerIds
        });
        _actionableThingRelatedEventRepository.Create(lotteryClosedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}