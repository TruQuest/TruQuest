using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;

namespace Application.Thing.Commands.ArchiveDueToFailedLottery;

[ExecuteInTxn]
public class ArchiveDueToFailedLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
}

internal class ArchiveDueToFailedLotteryCommandHandler : IRequestHandler<ArchiveDueToFailedLotteryCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public ArchiveDueToFailedLotteryCommandHandler(
        IThingRepository thingRepository,
        IThingUpdateRepository thingUpdateRepository,
        IWatchedItemRepository watchedItemRepository
    )
    {
        _thingRepository = thingRepository;
        _thingUpdateRepository = thingUpdateRepository;
        _watchedItemRepository = watchedItemRepository;
    }

    public async Task<VoidResult> Handle(ArchiveDueToFailedLotteryCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        if (thing.State == ThingState.FundedAndVerifierLotteryInitiated)
        {
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
        }

        return VoidResult.Instance;
    }
}