using MediatR;

using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.ThingAssessmentVerifierLottery.LotterySpotClaimed;

public class LotterySpotClaimedEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string UserId { get; init; }
}

internal class LotterySpotClaimedEventHandler : INotificationHandler<LotterySpotClaimedEvent>
{
    private readonly IThingAssessmentVerifierLotterySpotClaimedEventRepository _thingAssessmentVerifierLotterySpotClaimedEventRepository;

    public LotterySpotClaimedEventHandler(
        IThingAssessmentVerifierLotterySpotClaimedEventRepository thingAssessmentVerifierLotterySpotClaimedEventRepository
    )
    {
        _thingAssessmentVerifierLotterySpotClaimedEventRepository = thingAssessmentVerifierLotterySpotClaimedEventRepository;
    }

    public async Task Handle(LotterySpotClaimedEvent @event, CancellationToken ct)
    {
        var spotClaimedEvent = new ThingAssessmentVerifierLotterySpotClaimedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            userId: @event.UserId
        );
        _thingAssessmentVerifierLotterySpotClaimedEventRepository.Create(spotClaimedEvent);

        await _thingAssessmentVerifierLotterySpotClaimedEventRepository.SaveChanges();
    }
}