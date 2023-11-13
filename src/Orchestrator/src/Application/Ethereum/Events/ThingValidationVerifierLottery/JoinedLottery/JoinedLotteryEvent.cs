using GoThataway;

using Domain.Aggregates.Events;

using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.ThingValidationVerifierLottery.JoinedLottery;

public class JoinedLotteryEvent : BaseContractEvent, IEvent
{
    public required byte[] ThingId { get; init; }
    public required string WalletAddress { get; init; }
    public required byte[] UserData { get; init; }
    public required long L1BlockNumber { get; init; }
}

public class JoinedLotteryEventHandler : IEventHandler<JoinedLotteryEvent>
{
    private readonly IJoinedThingValidationVerifierLotteryEventRepository _joinedValidationVerifierLotteryEventRepository;

    public JoinedLotteryEventHandler(
        IJoinedThingValidationVerifierLotteryEventRepository joinedValidationVerifierLotteryEventRepository
    )
    {
        _joinedValidationVerifierLotteryEventRepository = joinedValidationVerifierLotteryEventRepository;
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
        _joinedValidationVerifierLotteryEventRepository.Create(joinedLotteryEvent);

        await _joinedValidationVerifierLotteryEventRepository.SaveChanges();
    }
}
