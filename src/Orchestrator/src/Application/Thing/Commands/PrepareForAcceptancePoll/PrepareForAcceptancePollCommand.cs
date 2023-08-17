using System.Diagnostics;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Common.Attributes;

namespace Application.Thing.Commands.PrepareForAcceptancePoll;

[ExecuteInTxn]
public class PrepareForAcceptancePollCommand : IRequest<VoidResult>
{
    public required long AcceptancePollInitBlockNumber { get; init; }
    public required int AcceptancePollInitTxnIndex { get; init; }
    public required string AcceptancePollInitTxnHash { get; init; }
    public required Guid ThingId { get; init; }
    public required string Orchestrator { get; init; }
    public required string Data { get; init; }
    public required string UserXorData { get; init; }
    public required string HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    public required List<string> WinnerIds { get; init; }
}

internal class PrepareForAcceptancePollCommandHandler : IRequestHandler<PrepareForAcceptancePollCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IJoinedThingSubmissionVerifierLotteryEventRepository _joinedThingSubmissionVerifierLotteryEventRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;
    private readonly IContractCaller _contractCaller;

    public PrepareForAcceptancePollCommandHandler(
        IThingRepository thingRepository,
        ITaskRepository taskRepository,
        IJoinedThingSubmissionVerifierLotteryEventRepository joinedThingSubmissionVerifierLotteryEventRepository,
        IThingUpdateRepository thingUpdateRepository,
        IWatchedItemRepository watchedItemRepository,
        IContractCaller contractCaller
    )
    {
        _thingRepository = thingRepository;
        _taskRepository = taskRepository;
        _joinedThingSubmissionVerifierLotteryEventRepository = joinedThingSubmissionVerifierLotteryEventRepository;
        _thingUpdateRepository = thingUpdateRepository;
        _watchedItemRepository = watchedItemRepository;
        _contractCaller = contractCaller;
    }

    public async Task<VoidResult> Handle(PrepareForAcceptancePollCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        if (thing.State == ThingState.FundedAndVerifierLotteryInitiated)
        {
            thing.SetState(ThingState.VerifiersSelectedAndPollInitiated);
            thing.AddVerifiers(command.WinnerIds);

            var lotteryInitBlock = await _contractCaller.GetThingSubmissionVerifierLotteryInitBlock(thing.Id.ToByteArray());
            Debug.Assert(lotteryInitBlock < 0);

            var pollInitBlock = await _contractCaller.GetThingAcceptancePollInitBlock(thing.Id.ToByteArray());
            int pollDurationBlocks = await _contractCaller.GetThingAcceptancePollDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingAcceptancePoll,
                scheduledBlockNumber: pollInitBlock + pollDurationBlocks + 1
            );
            task.SetPayload(new()
            {
                ["thingId"] = thing.Id
            });
            _taskRepository.Create(task);

            var joinedThingSubmissionVerifierLotteryEvent = new JoinedThingSubmissionVerifierLotteryEvent(
                blockNumber: command.AcceptancePollInitBlockNumber,
                txnIndex: command.AcceptancePollInitTxnIndex,
                txnHash: command.AcceptancePollInitTxnHash,
                thingId: command.ThingId,
                userId: command.Orchestrator,
                l1BlockNumber: -lotteryInitBlock,
                nonce: command.Nonce
            );
            _joinedThingSubmissionVerifierLotteryEventRepository.Create(joinedThingSubmissionVerifierLotteryEvent);

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            _watchedItemRepository.Add(
                command.WinnerIds
                    .Select(userId => new WatchedItem(
                        userId: userId,
                        itemType: WatchedItemType.Thing,
                        itemId: thing.Id,
                        itemUpdateCategory: (int)ThingUpdateCategory.Special,
                        lastSeenUpdateTimestamp: now
                    ))
                    .ToArray()
            );

            await _thingUpdateRepository.AddOrUpdate(
                new ThingUpdate(
                    thingId: thing.Id,
                    category: ThingUpdateCategory.General,
                    updateTimestamp: now + 10,
                    title: "Verifier lottery completed",
                    details: "Acceptance poll initiated"
                ),
                new ThingUpdate(
                    thingId: thing.Id,
                    category: ThingUpdateCategory.Special,
                    updateTimestamp: now + 10,
                    title: "You've been selected as a verifier!",
                    details: null
                )
            );

            await _thingRepository.SaveChanges();
            await _taskRepository.SaveChanges();
            await _joinedThingSubmissionVerifierLotteryEventRepository.SaveChanges();
            await _watchedItemRepository.SaveChanges();
            await _thingUpdateRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}
