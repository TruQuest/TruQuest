using MediatR;

using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.SettlementProposalAssessmentVerifierLottery.ClaimedLotterySpot;

public class ClaimedLotterySpotEvent : BaseContractEvent, INotification
{
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string WalletAddress { get; init; }
    public required long L1BlockNumber { get; init; }
}

internal class ClaimedLotterySpotEventHandler : INotificationHandler<ClaimedLotterySpotEvent>
{
    private readonly IThingValidationVerifierLotteryEventQueryable _thingLotteryEventQueryable;
    private readonly IClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository _claimedAssessmentVerifierLotterySpotEventRepository;

    public ClaimedLotterySpotEventHandler(
        IThingValidationVerifierLotteryEventQueryable thingLotteryEventQueryable,
        IClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository claimedAssessmentVerifierLotterySpotEventRepository
    )
    {
        _thingLotteryEventQueryable = thingLotteryEventQueryable;
        _claimedAssessmentVerifierLotterySpotEventRepository = claimedAssessmentVerifierLotterySpotEventRepository;
    }

    public async Task Handle(ClaimedLotterySpotEvent @event, CancellationToken ct)
    {
        var thingId = new Guid(@event.ThingId);
        var userData = await _thingLotteryEventQueryable.GetJoinedEventUserDataFor(thingId, @event.WalletAddress);

        var spotClaimedEvent = new ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            logIndex: @event.LogIndex,
            thingId: thingId,
            settlementProposalId: new Guid(@event.SettlementProposalId),
            walletAddress: @event.WalletAddress,
            l1BlockNumber: @event.L1BlockNumber,
            userData: userData
        );
        _claimedAssessmentVerifierLotterySpotEventRepository.Create(spotClaimedEvent);

        await _claimedAssessmentVerifierLotterySpotEventRepository.SaveChanges();
    }
}
