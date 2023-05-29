using Microsoft.Extensions.Logging;

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
    public required List<string> RewardedVerifiers { get; init; }
    public required List<string> SlashedVerifiers { get; init; }
}

internal class PollFinalizedEventHandler : INotificationHandler<PollFinalizedEvent>
{
    private readonly ILogger<PollFinalizedEventHandler> _logger;
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public PollFinalizedEventHandler(
        ILogger<PollFinalizedEventHandler> logger,
        IThingRepository thingRepository,
        IThingUpdateRepository thingUpdateRepository,
        IWatchedItemRepository watchedItemRepository
    )
    {
        _logger = logger;
        _thingRepository = thingRepository;
        _thingUpdateRepository = thingUpdateRepository;
        _watchedItemRepository = watchedItemRepository;
    }

    public async Task Handle(PollFinalizedEvent @event, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(new Guid(@event.ThingId));
        if (thing.State == ThingState.VerifiersSelectedAndPollInitiated)
        {
            var decision = (SubmissionEvaluationDecision)@event.Decision;
            _logger.LogInformation("Thing {ThingId} Acceptance Poll Decision: {Decision}", thing.Id, decision);

            if (decision is
                SubmissionEvaluationDecision.UnsettledDueToInsufficientVotingVolume or
                SubmissionEvaluationDecision.UnsettledDueToMajorityThresholdNotReached
            )
            {
                _logger.LogInformation("Rewarded verifiers: {Verifiers}", @event.RewardedVerifiers);
                _logger.LogInformation("Penalized verifiers: {Verifiers}", @event.SlashedVerifiers);

                thing.SetState(ThingState.ConsensusNotReached);
                thing.SetVoteAggIpfsCid(@event.VoteAggIpfsCid);

                await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                    thingId: thing.Id,
                    category: ThingUpdateCategory.General,
                    updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    title: "Promise acceptance poll",
                    details: "Consensus not reached"
                ));

                var thingCopyId = await _thingRepository.DeepCopyFromWith(
                    sourceThingId: thing.Id,
                    state: ThingState.AwaitingFunding
                );

                thing.SetRelatedThing(thingCopyId);

                await _watchedItemRepository.DuplicateGeneralItemsFrom(
                    WatchedItemType.Thing,
                    sourceItemId: thing.Id,
                    destItemId: thingCopyId
                );

                await _thingRepository.SaveChanges();
                await _thingUpdateRepository.SaveChanges();
                await _watchedItemRepository.SaveChanges();

                return;
            }
            else if (decision is SubmissionEvaluationDecision.Accepted)
            {
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
                return;
            }

            throw new NotImplementedException();
        }
    }
}