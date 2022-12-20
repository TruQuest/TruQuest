using MediatR;

using Domain.Aggregates.Events;
using PreJoinedVerifierLotteryEventDm = Domain.Aggregates.Events.PreJoinedVerifierLotteryEvent;

namespace Application.Ethereum.Events.Lottery.PreJoinedVerifierLottery;

public class PreJoinedVerifierLotteryEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required string UserId { get; init; }
    public required byte[] DataHash { get; init; }
}

internal class PreJoinedVerifierLotteryEventHandler : INotificationHandler<PreJoinedVerifierLotteryEvent>
{
    private readonly IPreJoinedVerifierLotteryEventRepository _preJoinedVerifierLotteryEventRepository;

    public PreJoinedVerifierLotteryEventHandler(IPreJoinedVerifierLotteryEventRepository preJoinedVerifierLotteryEventRepository)
    {
        _preJoinedVerifierLotteryEventRepository = preJoinedVerifierLotteryEventRepository;
    }

    public async Task Handle(PreJoinedVerifierLotteryEvent @event, CancellationToken ct)
    {
        var preJoinedVerifierLotteryEvent = new PreJoinedVerifierLotteryEventDm(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            userId: @event.UserId,
            dataHash: Convert.ToBase64String(@event.DataHash)
        );
        _preJoinedVerifierLotteryEventRepository.Create(preJoinedVerifierLotteryEvent);

        await _preJoinedVerifierLotteryEventRepository.SaveChanges();
    }
}