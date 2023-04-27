using MediatR;

using Domain.Aggregates.Events;
using PreJoinedThingSubmissionVerifierLotteryEventDm = Domain.Aggregates.Events.PreJoinedThingSubmissionVerifierLotteryEvent;

namespace Application.Ethereum.Events.ThingSubmissionVerifierLottery.PreJoinedLottery;

public class PreJoinedLotteryEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required string UserId { get; init; }
    public required byte[] DataHash { get; init; }
}

internal class PreJoinedLotteryEventHandler : INotificationHandler<PreJoinedLotteryEvent>
{
    private readonly IPreJoinedThingSubmissionVerifierLotteryEventRepository _preJoinedThingSubmissionVerifierLotteryEventRepository;

    public PreJoinedLotteryEventHandler(
        IPreJoinedThingSubmissionVerifierLotteryEventRepository preJoinedThingSubmissionVerifierLotteryEventRepository
    )
    {
        _preJoinedThingSubmissionVerifierLotteryEventRepository = preJoinedThingSubmissionVerifierLotteryEventRepository;
    }

    public async Task Handle(PreJoinedLotteryEvent @event, CancellationToken ct)
    {
        var preJoinedThingSubmissionVerifierLotteryEvent = new PreJoinedThingSubmissionVerifierLotteryEventDm(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            userId: @event.UserId,
            dataHash: Convert.ToBase64String(@event.DataHash)
        );
        _preJoinedThingSubmissionVerifierLotteryEventRepository.Create(preJoinedThingSubmissionVerifierLotteryEvent);

        await _preJoinedThingSubmissionVerifierLotteryEventRepository.SaveChanges();
    }
}