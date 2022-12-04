using MediatR;

using Domain.Results;
using Domain.Aggregates;

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

    public PrepareForAcceptancePollCommandHandler(
        IThingRepository thingRepository,
        ITaskRepository taskRepository
    )
    {
        _thingRepository = thingRepository;
        _taskRepository = taskRepository;
    }

    public async Task<VoidResult> Handle(PrepareForAcceptancePollCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindByIdHash(command.ThingIdHash);
        if (thing.State == ThingState.FundedAndVerifierLotteryInitiated)
        {
            thing.SetState(ThingState.VerifiersSelected);
            thing.AddVerifiers(command.WinnerIds);

            var task = new DeferredTask(
                type: TaskType.CloseThingAcceptancePoll,
                scheduledBlockNumber: command.AcceptancePollInitBlockNumber + 30
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