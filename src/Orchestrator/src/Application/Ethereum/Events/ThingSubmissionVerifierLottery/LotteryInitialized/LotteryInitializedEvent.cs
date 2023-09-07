using MediatR;

using Domain.Aggregates.Events;

using Application.Common.Misc;

namespace Application.Ethereum.Events.ThingSubmissionVerifierLottery.LotteryInitialized;

public class LotteryInitializedEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required string TxnHash { get; init; }
    public required long L1BlockNumber { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] DataHash { get; init; }
    public required byte[] UserXorDataHash { get; init; }
}

internal class LotteryInitializedEventHandler : INotificationHandler<LotteryInitializedEvent>
{
    private readonly IThingSubmissionVerifierLotteryInitializedEventRepository _thingSubmissionVerifierLotteryInitializedEventRepository;

    public LotteryInitializedEventHandler(
        IThingSubmissionVerifierLotteryInitializedEventRepository thingSubmissionVerifierLotteryInitializedEventRepository
    )
    {
        _thingSubmissionVerifierLotteryInitializedEventRepository = thingSubmissionVerifierLotteryInitializedEventRepository;
    }

    public async Task Handle(LotteryInitializedEvent @event, CancellationToken ct)
    {
        var lotteryInitializedEvent = new ThingSubmissionVerifierLotteryInitializedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            l1BlockNumber: @event.L1BlockNumber,
            thingId: new Guid(@event.ThingId),
            dataHash: @event.DataHash.ToHex(prefix: true),
            userXorDataHash: @event.UserXorDataHash.ToHex(prefix: true)
        );
        _thingSubmissionVerifierLotteryInitializedEventRepository.Create(lotteryInitializedEvent);

        await _thingSubmissionVerifierLotteryInitializedEventRepository.SaveChanges();
    }
}
