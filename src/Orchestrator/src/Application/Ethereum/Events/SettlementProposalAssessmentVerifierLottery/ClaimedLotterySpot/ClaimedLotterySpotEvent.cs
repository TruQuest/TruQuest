using GoThataway;

using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Ethereum.Common.Models.IM;
using Application.Common.Monitoring;

namespace Application.Ethereum.Events.SettlementProposalAssessmentVerifierLottery.ClaimedLotterySpot;

public class ClaimedLotterySpotEvent : BaseContractEvent, IEvent
{
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string WalletAddress { get; init; }
    public required long L1BlockNumber { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.SettlementProposalId, new Guid(SettlementProposalId)),
            (ActivityTags.WalletAddress, WalletAddress),
            (ActivityTags.TxnHash, TxnHash)
        };
    }
}

public class ClaimedLotterySpotEventHandler : IEventHandler<ClaimedLotterySpotEvent>
{
    private readonly IThingValidationVerifierLotteryEventQueryable _thingLotteryEventQueryable;
    private readonly IClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository _proposalLotteryEventRepository;

    public ClaimedLotterySpotEventHandler(
        IThingValidationVerifierLotteryEventQueryable thingLotteryEventQueryable,
        IClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository proposalLotteryEventRepository
    )
    {
        _thingLotteryEventQueryable = thingLotteryEventQueryable;
        _proposalLotteryEventRepository = proposalLotteryEventRepository;
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
        _proposalLotteryEventRepository.Create(spotClaimedEvent);

        await _proposalLotteryEventRepository.SaveChanges();
    }
}
