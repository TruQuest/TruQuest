using MediatR;

using Domain.Aggregates.Events;
using PreJoinedThingAssessmentVerifierLotteryEventDm = Domain.Aggregates.Events.PreJoinedThingAssessmentVerifierLotteryEvent;

namespace Application.Ethereum.Events.ThingAssessmentVerifierLottery.PreJoinedLottery;

public class PreJoinedLotteryEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
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
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            userId: @event.UserId,
            dataHash: Convert.ToBase64String(@event.DataHash)
        );
        _preJoinedThingAssessmentVerifierLotteryEventRepository.Create(preJoinedThingAssessmentVerifierLotteryEvent);

        await _preJoinedThingAssessmentVerifierLotteryEventRepository.SaveChanges();
    }
}