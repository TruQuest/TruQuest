using System.Numerics;

using MediatR;

using Domain.Aggregates.Events;
using JoinedVerifierLotteryEventDm = Domain.Aggregates.Events.JoinedVerifierLotteryEvent;

namespace Application.Ethereum.Events.Lottery.JoinedVerifierLottery;

public class JoinedVerifierLotteryEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required string UserId { get; init; }
    public BigInteger Nonce { get; init; }
}

internal class JoinedVerifierLotteryEventHandler : INotificationHandler<JoinedVerifierLotteryEvent>
{
    private readonly IJoinedVerifierLotteryEventRepository _joinedVerifierLotteryEventRepository;

    public JoinedVerifierLotteryEventHandler(IJoinedVerifierLotteryEventRepository joinedVerifierLotteryEventRepository)
    {
        _joinedVerifierLotteryEventRepository = joinedVerifierLotteryEventRepository;
    }

    public async Task Handle(JoinedVerifierLotteryEvent @event, CancellationToken ct)
    {
        var joinedVerifierLotteryEvent = new JoinedVerifierLotteryEventDm(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            userId: @event.UserId,
            nonce: (decimal)@event.Nonce
        );
        _joinedVerifierLotteryEventRepository.Create(joinedVerifierLotteryEvent);

        await _joinedVerifierLotteryEventRepository.SaveChanges();
    }
}