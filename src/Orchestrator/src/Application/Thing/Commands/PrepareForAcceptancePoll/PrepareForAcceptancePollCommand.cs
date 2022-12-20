using MediatR;

using Domain.Results;
using Domain.Aggregates;

using Application.Common.Interfaces;

namespace Application.Thing.Commands.PrepareForAcceptancePoll;

public class PrepareForAcceptancePollCommand : IRequest<VoidResult>
{
    public long AcceptancePollInitBlockNumber { get; init; }
    public required string ThingIdHash { get; init; }
    public required List<string> WinnerIds { get; init; }
}

internal class PrepareForAcceptancePollCommandHandler : IRequestHandler<PrepareForAcceptancePollCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public PrepareForAcceptancePollCommandHandler(
        IThingRepository thingRepository,
        ITaskRepository taskRepository,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _thingRepository = thingRepository;
        _taskRepository = taskRepository;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(PrepareForAcceptancePollCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindByIdHash(command.ThingIdHash);
        if (thing.State == ThingState.FundedAndVerifierLotteryInitiated)
        {
            thing.SetState(ThingState.VerifiersSelectedAndAcceptancePollInitiated);
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

            await _thingRepository.SaveChanges();
            await _taskRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}