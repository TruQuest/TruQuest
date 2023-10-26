using MediatR;

using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.ThingAssessmentVerifierLottery.ClaimedLotterySpot;

public class ClaimedLotterySpotEvent : BaseContractEvent, INotification
{
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string UserId { get; init; }
    public required long L1BlockNumber { get; init; }
}

internal class ClaimedLotterySpotEventHandler : INotificationHandler<ClaimedLotterySpotEvent>
{
    private readonly IThingSubmissionVerifierLotteryEventQueryable _thingLotteryEventQueryable;
    private readonly IThingAssessmentVerifierLotterySpotClaimedEventRepository _thingAssessmentVerifierLotterySpotClaimedEventRepository;

    public ClaimedLotterySpotEventHandler(
        IThingSubmissionVerifierLotteryEventQueryable thingLotteryEventQueryable,
        IThingAssessmentVerifierLotterySpotClaimedEventRepository thingAssessmentVerifierLotterySpotClaimedEventRepository
    )
    {
        _thingLotteryEventQueryable = thingLotteryEventQueryable;
        _thingAssessmentVerifierLotterySpotClaimedEventRepository = thingAssessmentVerifierLotterySpotClaimedEventRepository;
    }

    public async Task Handle(ClaimedLotterySpotEvent @event, CancellationToken ct)
    {
        var thingId = new Guid(@event.ThingId);
        var userData = await _thingLotteryEventQueryable.GetJoinedEventUserDataFor(thingId, @event.UserId);

        var spotClaimedEvent = new ThingAssessmentVerifierLotterySpotClaimedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            thingId: thingId,
            settlementProposalId: new Guid(@event.SettlementProposalId),
            userId: @event.UserId,
            l1BlockNumber: @event.L1BlockNumber,
            userData: userData
        );
        _thingAssessmentVerifierLotterySpotClaimedEventRepository.Create(spotClaimedEvent);

        await _thingAssessmentVerifierLotterySpotClaimedEventRepository.SaveChanges();
    }
}
