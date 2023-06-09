using MediatR;

using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.ThingAssessmentVerifierLottery.ClaimedLotterySpot;

public class ClaimedLotterySpotEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string UserId { get; init; }
    public required long L1BlockNumber { get; init; }
}

internal class ClaimedLotterySpotEventHandler : INotificationHandler<ClaimedLotterySpotEvent>
{
    private readonly IThingAssessmentVerifierLotterySpotClaimedEventRepository _thingAssessmentVerifierLotterySpotClaimedEventRepository;

    public ClaimedLotterySpotEventHandler(
        IThingAssessmentVerifierLotterySpotClaimedEventRepository thingAssessmentVerifierLotterySpotClaimedEventRepository
    )
    {
        _thingAssessmentVerifierLotterySpotClaimedEventRepository = thingAssessmentVerifierLotterySpotClaimedEventRepository;
    }

    public async Task Handle(ClaimedLotterySpotEvent @event, CancellationToken ct)
    {
        var spotClaimedEvent = new ThingAssessmentVerifierLotterySpotClaimedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            userId: @event.UserId,
            @event.L1BlockNumber
        );
        _thingAssessmentVerifierLotterySpotClaimedEventRepository.Create(spotClaimedEvent);

        await _thingAssessmentVerifierLotterySpotClaimedEventRepository.SaveChanges();
    }
}