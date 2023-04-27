using System.Numerics;

using MediatR;

using Domain.Aggregates.Events;
using JoinedThingAssessmentVerifierLotteryEventDm = Domain.Aggregates.Events.JoinedThingAssessmentVerifierLotteryEvent;

namespace Application.Ethereum.Events.ThingAssessmentVerifierLottery.JoinedLottery;

public class JoinedLotteryEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string UserId { get; init; }
    public required BigInteger Nonce { get; init; }
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
        var joinedThingAssessmentVerifierLotteryEvent = new JoinedThingAssessmentVerifierLotteryEventDm(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            userId: @event.UserId,
            nonce: (decimal)@event.Nonce
        );
        _joinedThingAssessmentVerifierLotteryEventRepository.Create(joinedThingAssessmentVerifierLotteryEvent);

        await _joinedThingAssessmentVerifierLotteryEventRepository.SaveChanges();
    }
}