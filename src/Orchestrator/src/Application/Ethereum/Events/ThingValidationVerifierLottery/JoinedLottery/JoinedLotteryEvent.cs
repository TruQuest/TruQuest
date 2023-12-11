using GoThataway;

using Domain.Aggregates.Events;

using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;
using Application.Common.Monitoring;

namespace Application.Ethereum.Events.ThingValidationVerifierLottery.JoinedLottery;

public class JoinedLotteryEvent : BaseContractEvent, IEvent
{
    public required byte[] ThingId { get; init; }
    public required string WalletAddress { get; init; }
    public required byte[] UserData { get; init; }
    public required long L1BlockNumber { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.ThingId, new Guid(ThingId)),
            (ActivityTags.WalletAddress, WalletAddress),
            (ActivityTags.TxnHash, TxnHash)
        };
    }
}

public class JoinedLotteryEventHandler : IEventHandler<JoinedLotteryEvent>
{
    private readonly IJoinedThingValidationVerifierLotteryEventRepository _lotteryEventRepository;

    public JoinedLotteryEventHandler(IJoinedThingValidationVerifierLotteryEventRepository lotteryEventRepository)
    {
        _lotteryEventRepository = lotteryEventRepository;
    }

    public async Task Handle(JoinedLotteryEvent @event, CancellationToken ct)
    {
        var joinedLotteryEvent = new JoinedThingValidationVerifierLotteryEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            logIndex: @event.LogIndex,
            thingId: new Guid(@event.ThingId),
            walletAddress: @event.WalletAddress,
            l1BlockNumber: @event.L1BlockNumber,
            userData: @event.UserData.ToHex(prefix: true)
        );
        _lotteryEventRepository.Create(joinedLotteryEvent);

        await _lotteryEventRepository.SaveChanges();
    }
}
