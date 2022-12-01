using MediatR;

using Domain.Aggregates.Events;
using PreJoinedLotteryEventDm = Domain.Aggregates.Events.PreJoinedLotteryEvent;

namespace Application.Ethereum.Events.Lottery.PreJoinedLottery;

public class PreJoinedLotteryEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required string ThingIdHash { get; init; }
    public required string UserId { get; init; }
    public required byte[] DataHash { get; init; }
}

internal class PreJoinedLotteryEventHandler : INotificationHandler<PreJoinedLotteryEvent>
{
    private readonly IPreJoinedLotteryEventRepository _preJoinedLotteryEventRepository;

    public PreJoinedLotteryEventHandler(IPreJoinedLotteryEventRepository preJoinedLotteryEventRepository)
    {
        _preJoinedLotteryEventRepository = preJoinedLotteryEventRepository;
    }

    public async Task Handle(PreJoinedLotteryEvent @event, CancellationToken ct)
    {
        var preJoinedLotteryEvent = new PreJoinedLotteryEventDm(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingIdHash: @event.ThingIdHash,
            userId: @event.UserId,
            dataHash: Convert.ToBase64String(@event.DataHash)
        );
        _preJoinedLotteryEventRepository.Create(preJoinedLotteryEvent);

        await _preJoinedLotteryEventRepository.SaveChanges();
    }
}