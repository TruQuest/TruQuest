using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Thing.Commands.ArchiveDueToFailedLottery;

[ExecuteInTxn]
public class ArchiveDueToFailedLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(VoidResult _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId),
        };
    }
}

public class ArchiveDueToFailedLotteryCommandHandler : IRequestHandler<ArchiveDueToFailedLotteryCommand, VoidResult>
{
    private readonly ILogger<ArchiveDueToFailedLotteryCommandHandler> _logger;
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public ArchiveDueToFailedLotteryCommandHandler(
        ILogger<ArchiveDueToFailedLotteryCommandHandler> logger,
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

    public async Task<VoidResult> Handle(ArchiveDueToFailedLotteryCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        if (thing.State != ThingState.FundedAndVerifierLotteryInitiated)
        {
            _logger.LogWarning($"Trying to archive an already archived thing {ThingId}", thing.Id);
            return VoidResult.Instance;
        }

        thing.SetState(ThingState.VerifierLotteryFailed);

        var thingCopyId = await _thingRepository.DeepCopyFromWith(
            sourceThingId: thing.Id,
            state: ThingState.AwaitingFunding
        );

        thing.AddRelatedThingAs(thingCopyId, relation: "next");

        await _thingUpdateRepository.AddOrUpdate(
            new ThingUpdate(
                thingId: thing.Id,
                category: ThingUpdateCategory.General,
                updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                title: "Verifier lottery failed",
                details: "Not enough participants"
            )
        );

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

        _logger.LogInformation($"Archived thing {ThingId} due to failed validation verifier lottery", thing.Id);

        return VoidResult.Instance;
    }
}
