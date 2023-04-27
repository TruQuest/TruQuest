using System.Numerics;

using MediatR;

using Domain.Aggregates.Events;
using JoinedThingSubmissionVerifierLotteryEventDm = Domain.Aggregates.Events.JoinedThingSubmissionVerifierLotteryEvent;

namespace Application.Ethereum.Events.ThingSubmissionVerifierLottery.JoinedLottery;

public class JoinedLotteryEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required string UserId { get; init; }
    public required BigInteger Nonce { get; init; }
}

internal class JoinedLotteryEventHandler : INotificationHandler<JoinedLotteryEvent>
{
    private readonly IJoinedThingSubmissionVerifierLotteryEventRepository _joinedThingSubmissionVerifierLotteryEventRepository;

    public JoinedLotteryEventHandler(
        IJoinedThingSubmissionVerifierLotteryEventRepository joinedThingSubmissionVerifierLotteryEventRepository
    )
    {
        _joinedThingSubmissionVerifierLotteryEventRepository = joinedThingSubmissionVerifierLotteryEventRepository;
    }

    public async Task Handle(JoinedLotteryEvent @event, CancellationToken ct)
    {
        var joinedThingSubmissionVerifierLotteryEvent = new JoinedThingSubmissionVerifierLotteryEventDm(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            userId: @event.UserId,
            nonce: (decimal)@event.Nonce
        );
        _joinedThingSubmissionVerifierLotteryEventRepository.Create(joinedThingSubmissionVerifierLotteryEvent);

        await _joinedThingSubmissionVerifierLotteryEventRepository.SaveChanges();
    }
}