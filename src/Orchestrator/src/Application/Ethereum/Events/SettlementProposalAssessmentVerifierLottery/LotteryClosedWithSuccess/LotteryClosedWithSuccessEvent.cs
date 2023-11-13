using GoThataway;

using Domain.Aggregates.Events;

using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.SettlementProposalAssessmentVerifierLottery.LotteryClosedWithSuccess;

public class LotteryClosedWithSuccessEvent : BaseContractEvent, IEvent
{
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string Orchestrator { get; init; }
    public required byte[] Data { get; init; }
    public required byte[] UserXorData { get; init; }
    public required byte[] HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    public required List<string> ClaimantWalletAddresses { get; init; }
    public required List<string> WinnerWalletAddresses { get; init; }
}

public class LotteryClosedWithSuccessEventHandler : IEventHandler<LotteryClosedWithSuccessEvent>
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
            txnHash: @event.TxnHash,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.SettlementProposalAssessmentVerifierLotterySucceeded
        );

        var payload = new Dictionary<string, object>()
        {
            ["settlementProposalId"] = new Guid(@event.SettlementProposalId),
            ["orchestrator"] = @event.Orchestrator,
            ["data"] = @event.Data.ToHex(prefix: true),
            ["userXorData"] = @event.UserXorData.ToHex(prefix: true),
            ["hashOfL1EndBlock"] = @event.HashOfL1EndBlock.ToHex(prefix: true),
            ["nonce"] = @event.Nonce,
            ["claimantWalletAddresses"] = @event.ClaimantWalletAddresses,
            ["winnerWalletAddresses"] = @event.WinnerWalletAddresses
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        lotteryClosedEvent.SetPayload(payload);

        _actionableThingRelatedEventRepository.Create(lotteryClosedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}
