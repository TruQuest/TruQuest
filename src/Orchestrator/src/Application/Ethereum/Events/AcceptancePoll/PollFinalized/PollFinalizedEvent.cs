using MediatR;

using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.AcceptancePoll.PollFinalized;

public class PollFinalizedEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required string TxnHash { get; init; }
    public required byte[] ThingId { get; init; }
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public required List<string> RewardedVerifiers { get; init; }
    public required List<string> SlashedVerifiers { get; init; }
}

internal class PollFinalizedEventHandler : INotificationHandler<PollFinalizedEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public PollFinalizedEventHandler(IActionableThingRelatedEventRepository actionableThingRelatedEventRepository)
    {
        _actionableThingRelatedEventRepository = actionableThingRelatedEventRepository;
    }

    public async Task Handle(PollFinalizedEvent @event, CancellationToken ct)
    {
        var pollFinalizedEvent = new ActionableThingRelatedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.AcceptancePollFinalized
        );
        pollFinalizedEvent.SetPayload(new()
        {
            ["decision"] = @event.Decision,
            ["voteAggIpfsCid"] = @event.VoteAggIpfsCid,
            ["rewardedVerifiers"] = @event.RewardedVerifiers,
            ["slashedVerifiers"] = @event.SlashedVerifiers,
        });
        _actionableThingRelatedEventRepository.Create(pollFinalizedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}
