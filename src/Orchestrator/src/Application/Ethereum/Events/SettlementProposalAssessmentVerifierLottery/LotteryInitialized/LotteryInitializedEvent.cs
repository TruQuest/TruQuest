using GoThataway;

using Domain.Aggregates.Events;

using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.SettlementProposalAssessmentVerifierLottery.LotteryInitialized;

public class LotteryInitializedEvent : BaseContractEvent, IEvent
{
    public required long L1BlockNumber { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required byte[] DataHash { get; init; }
    public required byte[] UserXorDataHash { get; init; }
}

public class LotteryInitializedEventHandler : IEventHandler<LotteryInitializedEvent>
{
    private readonly ISettlementProposalAssessmentVerifierLotteryInitializedEventRepository _assessmentVerifierLotteryInitializedEventRepository;

    public LotteryInitializedEventHandler(
        ISettlementProposalAssessmentVerifierLotteryInitializedEventRepository assessmentVerifierLotteryInitializedEventRepository
    )
    {
        _assessmentVerifierLotteryInitializedEventRepository = assessmentVerifierLotteryInitializedEventRepository;
    }

    public async Task Handle(LotteryInitializedEvent @event, CancellationToken ct)
    {
        var lotteryInitializedEvent = new SettlementProposalAssessmentVerifierLotteryInitializedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            l1BlockNumber: @event.L1BlockNumber,
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            dataHash: @event.DataHash.ToHex(prefix: true),
            userXorDataHash: @event.UserXorDataHash.ToHex(prefix: true)
        );
        _assessmentVerifierLotteryInitializedEventRepository.Create(lotteryInitializedEvent);

        await _assessmentVerifierLotteryInitializedEventRepository.SaveChanges();
    }
}

