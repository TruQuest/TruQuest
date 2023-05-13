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
    public required Guid ThingId { get; init; }
    public required string Orchestrator { get; init; }
    public required decimal Nonce { get; init; }
    public required List<string> WinnerIds { get; init; }
}

internal class PrepareForAcceptancePollCommandHandler : IRequestHandler<PrepareForAcceptancePollCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IJoinedThingSubmissionVerifierLotteryEventRepository _joinedThingSubmissionVerifierLotteryEventRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public PrepareForAcceptancePollCommandHandler(
        IThingRepository thingRepository,
        ITaskRepository taskRepository,
        IJoinedThingSubmissionVerifierLotteryEventRepository joinedThingSubmissionVerifierLotteryEventRepository,
        IThingUpdateRepository thingUpdateRepository,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _thingRepository = thingRepository;
        _taskRepository = taskRepository;
        _joinedThingSubmissionVerifierLotteryEventRepository = joinedThingSubmissionVerifierLotteryEventRepository;
        _thingUpdateRepository = thingUpdateRepository;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(PrepareForAcceptancePollCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        if (thing.State == ThingState.FundedAndVerifierLotteryInitiated)
        {
            thing.SetState(ThingState.VerifiersSelectedAndPollInitiated);
            thing.AddVerifiers(command.WinnerIds);

            int pollDurationBlocks = await _contractStorageQueryable.GetAcceptancePollDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingAcceptancePoll,
                scheduledBlockNumber: command.AcceptancePollInitBlockNumber + pollDurationBlocks
            );
            task.SetPayload(new()
            {
                ["thingId"] = thing.Id
            });
            _taskRepository.Create(task);

            var joinedThingSubmissionVerifierLotteryEvent = new JoinedThingSubmissionVerifierLotteryEvent(
                blockNumber: command.AcceptancePollInitBlockNumber,
                txnIndex: command.AcceptancePollInitTxnIndex,
                thingId: command.ThingId,
                userId: command.Orchestrator,
                nonce: command.Nonce
            );
            _joinedThingSubmissionVerifierLotteryEventRepository.Create(joinedThingSubmissionVerifierLotteryEvent);

            await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                thingId: thing.Id,
                category: ThingUpdateCategory.General,
                updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                title: "Thing verifier lottery completed",
                details: "Acceptance poll initiated"
            ));

            await _thingRepository.SaveChanges();
            await _taskRepository.SaveChanges();
            await _joinedThingSubmissionVerifierLotteryEventRepository.SaveChanges();
            await _thingUpdateRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}