using System.Numerics;

using MediatR;

using Domain.Aggregates.Events;
using JoinedLotteryEventDm = Domain.Aggregates.Events.JoinedLotteryEvent;

namespace Application.Ethereum.Events.Lottery.JoinedLottery;

public class JoinedLotteryEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required string ThingIdHash { get; init; }
    public required string UserId { get; init; }
    public BigInteger Nonce { get; init; }
}

internal class JoinedLotteryEventHandler : INotificationHandler<JoinedLotteryEvent>
{
    private readonly IJoinedLotteryEventRepository _joinedLotteryEventRepository;

    public JoinedLotteryEventHandler(IJoinedLotteryEventRepository joinedLotteryEventRepository)
    {
        _joinedLotteryEventRepository = joinedLotteryEventRepository;
    }

    public async Task Handle(JoinedLotteryEvent @event, CancellationToken ct)
    {
        var joinedLotteryEvent = new JoinedLotteryEventDm(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingIdHash: @event.ThingIdHash,
            userId: @event.UserId,
            nonce: (decimal)@event.Nonce
        );
        _joinedLotteryEventRepository.Create(joinedLotteryEvent);

        await _joinedLotteryEventRepository.SaveChanges();
    }
}