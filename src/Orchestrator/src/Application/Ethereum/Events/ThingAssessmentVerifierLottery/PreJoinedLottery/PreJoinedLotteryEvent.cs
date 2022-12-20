using MediatR;

using Domain.Aggregates.Events;
using PreJoinedThingAssessmentVerifierLotteryEventDm = Domain.Aggregates.Events.PreJoinedThingAssessmentVerifierLotteryEvent;

namespace Application.Ethereum.Events.ThingAssessmentVerifierLottery.PreJoinedLottery;

public class PreJoinedLotteryEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required string ThingIdHash { get; init; }
    public required string SettlementProposalIdHash { get; init; }
    public required string UserId { get; init; }
    public required byte[] DataHash { get; init; }
}

internal class PreJoinedLotteryEventHandler : INotificationHandler<PreJoinedLotteryEvent>
{
    private readonly IPreJoinedThingAssessmentVerifierLotteryEventRepository _preJoinedThingAssessmentVerifierLotteryEventRepository;

    public PreJoinedLotteryEventHandler(
        IPreJoinedThingAssessmentVerifierLotteryEventRepository preJoinedThingAssessmentVerifierLotteryEventRepository
    )
    {
        _preJoinedThingAssessmentVerifierLotteryEventRepository = preJoinedThingAssessmentVerifierLotteryEventRepository;
    }

    public async Task Handle(PreJoinedLotteryEvent @event, CancellationToken ct)
    {
        var preJoinedThingAssessmentVerifierLotteryEvent = new PreJoinedThingAssessmentVerifierLotteryEventDm(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingIdHash: @event.ThingIdHash,
            settlementProposalIdHash: @event.SettlementProposalIdHash,
            userId: @event.UserId,
            dataHash: Convert.ToBase64String(@event.DataHash)
        );
        _preJoinedThingAssessmentVerifierLotteryEventRepository.Create(preJoinedThingAssessmentVerifierLotteryEvent);

        await _preJoinedThingAssessmentVerifierLotteryEventRepository.SaveChanges();
    }
}