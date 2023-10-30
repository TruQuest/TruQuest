using MediatR;

using Domain.Aggregates.Events;

using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.ThingValidationVerifierLottery.LotteryInitialized;

public class LotteryInitializedEvent : BaseContractEvent, INotification
{
    public required long L1BlockNumber { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] DataHash { get; init; }
    public required byte[] UserXorDataHash { get; init; }
}

internal class LotteryInitializedEventHandler : INotificationHandler<LotteryInitializedEvent>
{
    private readonly IThingValidationVerifierLotteryInitializedEventRepository _validationVerifierLotteryInitializedEventRepository;

    public LotteryInitializedEventHandler(
        IThingValidationVerifierLotteryInitializedEventRepository validationVerifierLotteryInitializedEventRepository
    )
    {
        _validationVerifierLotteryInitializedEventRepository = validationVerifierLotteryInitializedEventRepository;
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
        _validationVerifierLotteryInitializedEventRepository.Create(lotteryInitializedEvent);

        await _validationVerifierLotteryInitializedEventRepository.SaveChanges();
    }
}
