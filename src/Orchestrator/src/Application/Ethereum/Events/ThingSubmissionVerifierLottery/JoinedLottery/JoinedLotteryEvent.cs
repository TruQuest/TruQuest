using MediatR;

using Domain.Aggregates.Events;

using Application.Common.Misc;

namespace Application.Ethereum.Events.ThingSubmissionVerifierLottery.JoinedLottery;

public class JoinedLotteryEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required string UserId { get; init; }
    public required byte[] UserData { get; init; }
    public required long L1BlockNumber { get; init; }
}

internal class JoinedLotteryEventHandler : INotificationHandler<JoinedLotteryEvent>
{
    private readonly IJoinedThingSubmissionVerifierLotteryEventRepository _joinedThingSubmissionVerifierLotteryEventRepository;

    public JoinedLotteryEventHandler(
        IJoinedThingSubmissionVerifierLotteryEventRepository joinedThingSubmissionVerifierLotteryEventRepository
    )
    {
        _joinedThingSubmissionVerifierLotteryEventRepository = joinedThingSubmissionVerifierLotteryEventRepository;
    }

    public async Task Handle(JoinedLotteryEvent @event, CancellationToken ct)
    {
        var joinedThingSubmissionVerifierLotteryEvent = new JoinedThingSubmissionVerifierLotteryEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            userId: @event.UserId,
            l1BlockNumber: @event.L1BlockNumber,
            userData: @event.UserData.ToHex(prefix: true)
        );
        _joinedThingSubmissionVerifierLotteryEventRepository.Create(joinedThingSubmissionVerifierLotteryEvent);

        await _joinedThingSubmissionVerifierLotteryEventRepository.SaveChanges();
    }
}