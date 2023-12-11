using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Thing.Commands.FinalizeValidationPoll;

[ExecuteInTxn]
public class FinalizeValidationPollCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required ValidationDecision Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public required List<string> RewardedVerifiers { get; init; }
    public required List<string> SlashedVerifiers { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(VoidResult _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId),
            (ActivityTags.IpfsCid, VoteAggIpfsCid),
            (ActivityTags.Decision, Decision.ToString())
        };
    }
}

public class FinalizeValidationPollCommandHandler : IRequestHandler<FinalizeValidationPollCommand, VoidResult>
{
    private readonly ILogger<FinalizeValidationPollCommandHandler> _logger;
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public FinalizeValidationPollCommandHandler(
        ILogger<FinalizeValidationPollCommandHandler> logger,
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

    public async Task<VoidResult> Handle(FinalizeValidationPollCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        if (thing.State != ThingState.VerifiersSelectedAndPollInitiated)
        {
            _logger.LogWarning($"Trying to finalize an already finalized thing {ThingId} validation poll", thing.Id);
            return VoidResult.Instance;
        }

        var decision = command.Decision;

        if (decision is
            ValidationDecision.UnsettledDueToInsufficientVotingVolume or
            ValidationDecision.UnsettledDueToMajorityThresholdNotReached
        )
        {
            thing.SetState(ThingState.ConsensusNotReached);
            thing.SetVoteAggIpfsCid(command.VoteAggIpfsCid);

            await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                thingId: thing.Id,
                category: ThingUpdateCategory.General,
                updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                title: "Promise validation poll",
                details: "Consensus not reached"
            ));

            var thingCopyId = await _thingRepository.DeepCopyFromWith(
                sourceThingId: thing.Id,
                state: ThingState.AwaitingFunding
            );

            thing.AddRelatedThingAs(thingCopyId, relation: "next");

            await _watchedItemRepository.DuplicateGeneralItemsFrom(
                WatchedItemType.Thing,
                sourceItemId: thing.Id,
                destItemId: thingCopyId
            );

            await _thingRepository.SaveChanges();
            await _thingUpdateRepository.SaveChanges();
            await _watchedItemRepository.SaveChanges();

            _logger.LogInformation($"Created a new thing {ThingId} as a deep copy of existing thing {RelatedThingId}", thingCopyId, thing.Id);
            Telemetry.CurrentActivity?.SetTag(ActivityTags.RelatedThingId, thingCopyId);
        }
        else if (decision is ValidationDecision.Accepted)
        {
            thing.SetState(ThingState.AwaitingSettlement);
            thing.SetVoteAggIpfsCid(command.VoteAggIpfsCid);

            await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                thingId: thing.Id,
                category: ThingUpdateCategory.General,
                updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                title: "Promise validation poll completed",
                details: "Awaiting settlement"
            ));

            await _thingRepository.SaveChanges();
            await _thingUpdateRepository.SaveChanges();
        }
        else if (decision is
            ValidationDecision.SoftDeclined or
            ValidationDecision.HardDeclined
        )
        {
            thing.SetState(ThingState.Declined);
            thing.SetVoteAggIpfsCid(command.VoteAggIpfsCid);

            await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                thingId: thing.Id,
                category: ThingUpdateCategory.General,
                updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                title: "Promise validation poll completed",
                details: "Submission declined"
            ));

            await _thingRepository.SaveChanges();
            await _thingUpdateRepository.SaveChanges();
        }

        _logger.LogInformation(
            $"Finalized thing {ThingId} validation poll: set appropriate state, added aggregate vote IPFS cid, etc",
            thing.Id
        );

        return VoidResult.Instance;
    }
}
