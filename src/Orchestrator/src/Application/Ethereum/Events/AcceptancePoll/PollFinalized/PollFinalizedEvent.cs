using MediatR;

using Domain.Aggregates;

using Application.Common.Interfaces;

namespace Application.Ethereum.Events.AcceptancePoll.PollFinalized;

public class PollFinalizedEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
}

internal class PollFinalizedEventHandler : INotificationHandler<PollFinalizedEvent>
{
    private readonly IThingRepository _thingRepository;
    private readonly IClientNotifier _clientNotifier;

    public PollFinalizedEventHandler(IThingRepository thingRepository, IClientNotifier clientNotifier)
    {
        _thingRepository = thingRepository;
        _clientNotifier = clientNotifier;
    }

    public async Task Handle(PollFinalizedEvent @event, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(new Guid(@event.ThingId));
        if (thing.State == ThingState.VerifiersSelectedAndPollInitiated)
        {
            var decision = (SubmissionEvaluationDecision)@event.Decision;
            System.Diagnostics.Debug.Assert(decision == SubmissionEvaluationDecision.Accepted);

            thing.SetState(ThingState.AwaitingSettlement);
            thing.SetVoteAggIpfsCid(@event.VoteAggIpfsCid);
            await _thingRepository.SaveChanges();

            await _clientNotifier.NotifyThingStateChanged(thing.Id, thing.State);
        }
    }
}