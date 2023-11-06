using System.Diagnostics;

using MediatR;

using Domain.Aggregates.Events;

using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;
using Application.Common.Interfaces;

namespace Application.Ethereum.Events.ThingValidationVerifierLottery.JoinedLottery;

public class JoinedLotteryEvent : BaseContractEvent, INotification
{
    public required byte[] ThingId { get; init; }
    public required string WalletAddress { get; init; }
    public required byte[] UserData { get; init; }
    public required long L1BlockNumber { get; init; }
}

internal class JoinedLotteryEventHandler : INotificationHandler<JoinedLotteryEvent>
{
    private readonly IJoinedThingValidationVerifierLotteryEventRepository _joinedValidationVerifierLotteryEventRepository;
#if DEBUG
    private readonly IEthereumAddressFormatter _ethereumAddressFormatter;
#endif

    public JoinedLotteryEventHandler(
        IJoinedThingValidationVerifierLotteryEventRepository joinedValidationVerifierLotteryEventRepository
#if DEBUG
        , IEthereumAddressFormatter ethereumAddressFormatter
#endif
    )
    {
        _joinedValidationVerifierLotteryEventRepository = joinedValidationVerifierLotteryEventRepository;
#if DEBUG
        _ethereumAddressFormatter = ethereumAddressFormatter;
#endif
    }

    public async Task Handle(JoinedLotteryEvent @event, CancellationToken ct)
    {
        Debug.Assert(_ethereumAddressFormatter.IsValidEIP55EncodedAddress(@event.WalletAddress));

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
