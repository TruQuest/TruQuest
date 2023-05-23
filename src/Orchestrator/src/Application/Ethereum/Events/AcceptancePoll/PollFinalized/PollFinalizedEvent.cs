using MediatR;

using Domain.Aggregates;

using Application.Common.Attributes;

namespace Application.Ethereum.Events.AcceptancePoll.PollFinalized;

[ExecuteInTxn]
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
    private readonly IThingUpdateRepository _thingUpdateRepository;

    public PollFinalizedEventHandler(
        IThingRepository thingRepository,
        IThingUpdateRepository thingUpdateRepository
    )
    {
        _thingRepository = thingRepository;
        _thingUpdateRepository = thingUpdateRepository;
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

            await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                thingId: thing.Id,
                category: ThingUpdateCategory.General,
                updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                title: "Promise acceptance poll completed",
                details: "Awaiting settlement"
            ));

            await _thingRepository.SaveChanges();
            await _thingUpdateRepository.SaveChanges();
        }
    }
}