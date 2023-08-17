using MediatR;

using Domain.Aggregates.Events;

using Application.Common.Misc;

namespace Application.Ethereum.Events.ThingAssessmentVerifierLottery.JoinedLottery;

public class JoinedLotteryEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required string TxnHash { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string UserId { get; init; }
    public required byte[] UserData { get; init; }
    public required long L1BlockNumber { get; init; }
}

internal class JoinedLotteryEventHandler : INotificationHandler<JoinedLotteryEvent>
{
    private readonly IJoinedThingAssessmentVerifierLotteryEventRepository _joinedThingAssessmentVerifierLotteryEventRepository;

    public JoinedLotteryEventHandler(IJoinedThingAssessmentVerifierLotteryEventRepository joinedThingAssessmentVerifierLotteryEventRepository)
    {
        _joinedThingAssessmentVerifierLotteryEventRepository = joinedThingAssessmentVerifierLotteryEventRepository;
    }

    public async Task Handle(JoinedLotteryEvent @event, CancellationToken ct)
    {
        var joinedThingAssessmentVerifierLotteryEvent = new JoinedThingAssessmentVerifierLotteryEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            userId: @event.UserId,
            l1BlockNumber: @event.L1BlockNumber,
            userData: @event.UserData.ToHex(prefix: true)
        );
        _joinedThingAssessmentVerifierLotteryEventRepository.Create(joinedThingAssessmentVerifierLotteryEvent);

        await _joinedThingAssessmentVerifierLotteryEventRepository.SaveChanges();
    }
}
