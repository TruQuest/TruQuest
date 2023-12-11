using GoThataway;

using Domain.Aggregates.Events;

using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;
using Application.Common.Monitoring;

namespace Application.Ethereum.Events.ThingValidationVerifierLottery.LotteryInitialized;

public class LotteryInitializedEvent : BaseContractEvent, IEvent
{
    public required long L1BlockNumber { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] DataHash { get; init; }
    public required byte[] UserXorDataHash { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.ThingId, new Guid(ThingId)),
            (ActivityTags.TxnHash, TxnHash)
        };
    }
}

public class LotteryInitializedEventHandler : IEventHandler<LotteryInitializedEvent>
{
    private readonly IThingValidationVerifierLotteryInitializedEventRepository _lotteryEventRepository;

    public LotteryInitializedEventHandler(IThingValidationVerifierLotteryInitializedEventRepository lotteryEventRepository)
    {
        _lotteryEventRepository = lotteryEventRepository;
    }

    public async Task Handle(LotteryInitializedEvent @event, CancellationToken ct)
    {
        var lotteryInitializedEvent = new ThingValidationVerifierLotteryInitializedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            l1BlockNumber: @event.L1BlockNumber,
            thingId: new Guid(@event.ThingId),
            dataHash: @event.DataHash.ToHex(prefix: true),
            userXorDataHash: @event.UserXorDataHash.ToHex(prefix: true)
        );
        _lotteryEventRepository.Create(lotteryInitializedEvent);

        await _lotteryEventRepository.SaveChanges();
    }
}
