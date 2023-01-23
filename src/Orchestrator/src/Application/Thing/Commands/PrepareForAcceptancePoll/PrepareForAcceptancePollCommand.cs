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
    private readonly IContractStorageQueryable _contractStorageQueryable;
    private readonly IClientNotifier _clientNotifier;

    public PrepareForAcceptancePollCommandHandler(
        IThingRepository thingRepository,
        ITaskRepository taskRepository,
        IJoinedThingSubmissionVerifierLotteryEventRepository joinedThingSubmissionVerifierLotteryEventRepository,
        IContractStorageQueryable contractStorageQueryable,
        IClientNotifier clientNotifier
    )
    {
        _thingRepository = thingRepository;
        _taskRepository = taskRepository;
        _joinedThingSubmissionVerifierLotteryEventRepository = joinedThingSubmissionVerifierLotteryEventRepository;
        _contractStorageQueryable = contractStorageQueryable;
        _clientNotifier = clientNotifier;
    }

    public async Task<VoidResult> Handle(PrepareForAcceptancePollCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        if (thing.State == ThingState.FundedAndSubmissionVerifierLotteryInitiated)
        {
            thing.SetState(ThingState.SubmissionVerifiersSelectedAndPollInitiated);
            thing.AddVerifiers(command.WinnerIds);

            int pollDurationBlocks = await _contractStorageQueryable.GetAcceptancePollDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingAcceptancePoll,
                scheduledBlockNumber: command.AcceptancePollInitBlockNumber + pollDurationBlocks
            );
            task.SetPayload(new()
            {
                ["thingId"] = thing.Id!
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

            await _thingRepository.SaveChanges();
            await _taskRepository.SaveChanges();
            await _joinedThingSubmissionVerifierLotteryEventRepository.SaveChanges();

            await _clientNotifier.NotifyThingStateChanged(thing.Id, thing.State);
        }

        return VoidResult.Instance;
    }
}