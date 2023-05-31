using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;

namespace Application.Thing.Commands.FinalizeAcceptancePoll;

[ExecuteInTxn]
public class FinalizeAcceptancePollCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required SubmissionEvaluationDecision Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public required List<string> RewardedVerifiers { get; init; }
    public required List<string> SlashedVerifiers { get; init; }
}

internal class FinalizeAcceptancePollCommandHandler : IRequestHandler<FinalizeAcceptancePollCommand, VoidResult>
{
    private readonly ILogger<FinalizeAcceptancePollCommandHandler> _logger;
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public FinalizeAcceptancePollCommandHandler(
        ILogger<FinalizeAcceptancePollCommandHandler> logger,
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

    public async Task<VoidResult> Handle(FinalizeAcceptancePollCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        if (thing.State == ThingState.VerifiersSelectedAndPollInitiated)
        {
            var decision = command.Decision;
            _logger.LogInformation("Thing {ThingId} Acceptance Poll Decision: {Decision}", thing.Id, decision);
            _logger.LogInformation("Rewarded verifiers: {Verifiers}", command.RewardedVerifiers);
            _logger.LogInformation("Penalized verifiers: {Verifiers}", command.SlashedVerifiers);

            if (decision is
                SubmissionEvaluationDecision.UnsettledDueToInsufficientVotingVolume or
                SubmissionEvaluationDecision.UnsettledDueToMajorityThresholdNotReached
            )
            {
                thing.SetState(ThingState.ConsensusNotReached);
                thing.SetVoteAggIpfsCid(command.VoteAggIpfsCid);

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
            }
            else if (decision is SubmissionEvaluationDecision.Accepted)
            {
                thing.SetState(ThingState.AwaitingSettlement);
                thing.SetVoteAggIpfsCid(command.VoteAggIpfsCid);

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
            else if (decision is
                SubmissionEvaluationDecision.SoftDeclined or
                SubmissionEvaluationDecision.HardDeclined
            )
            {
                thing.SetState(ThingState.Declined);
                thing.SetVoteAggIpfsCid(command.VoteAggIpfsCid);

                await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                    thingId: thing.Id,
                    category: ThingUpdateCategory.General,
                    updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    title: "Promise acceptance poll completed",
                    details: "Submission declined"
                ));

                await _thingRepository.SaveChanges();
                await _thingUpdateRepository.SaveChanges();
            }
        }

        return VoidResult.Instance;
    }
}