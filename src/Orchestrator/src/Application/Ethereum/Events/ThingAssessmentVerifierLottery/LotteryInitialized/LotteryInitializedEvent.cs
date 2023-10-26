using MediatR;

using Domain.Aggregates.Events;

using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.ThingAssessmentVerifierLottery.LotteryInitialized;

public class LotteryInitializedEvent : BaseContractEvent, INotification
{
    public required long L1BlockNumber { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required byte[] DataHash { get; init; }
    public required byte[] UserXorDataHash { get; init; }
}

internal class LotteryInitializedEventHandler : INotificationHandler<LotteryInitializedEvent>
{
    private readonly IThingAssessmentVerifierLotteryInitializedEventRepository _thingAssessmentVerifierLotteryInitializedEventRepository;

    public LotteryInitializedEventHandler(
        IThingAssessmentVerifierLotteryInitializedEventRepository thingAssessmentVerifierLotteryInitializedEventRepository
    )
    {
        _thingAssessmentVerifierLotteryInitializedEventRepository = thingAssessmentVerifierLotteryInitializedEventRepository;
    }

    public async Task Handle(LotteryInitializedEvent @event, CancellationToken ct)
    {
        var lotteryInitializedEvent = new ThingAssessmentVerifierLotteryInitializedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            l1BlockNumber: @event.L1BlockNumber,
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            dataHash: @event.DataHash.ToHex(prefix: true),
            userXorDataHash: @event.UserXorDataHash.ToHex(prefix: true)
        );
        _thingAssessmentVerifierLotteryInitializedEventRepository.Create(lotteryInitializedEvent);

        await _thingAssessmentVerifierLotteryInitializedEventRepository.SaveChanges();
    }
}

