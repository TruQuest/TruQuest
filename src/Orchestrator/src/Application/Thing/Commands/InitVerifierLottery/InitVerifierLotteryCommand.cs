using System.Security.Cryptography;

using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Thing.Commands.InitVerifierLottery;

[ExecuteInTxn]
public class InitVerifierLotteryCommand : IRequest<VoidResult>
{
    public Guid ThingId { get; init; }
}

internal class InitVerifierLotteryCommandHandler : IRequestHandler<InitVerifierLotteryCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractCaller _contractCaller;

    public InitVerifierLotteryCommandHandler(
        IThingRepository thingRepository,
        ITaskRepository taskRepository,
        IContractCaller contractCaller
    )
    {
        _thingRepository = thingRepository;
        _taskRepository = taskRepository;
        _contractCaller = contractCaller;
    }

    public async Task<VoidResult> Handle(InitVerifierLotteryCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        if (thing.State == ThingState.Funded)
        {
            var data = RandomNumberGenerator.GetBytes(32);
            var dataHash = await _contractCaller.ComputeHash(data);

            await _contractCaller.InitVerifierLottery(thing.Id!.Value.ToString(), dataHash);

            var task = new DeferredTask(
                type: TaskType.CloseVerifierLottery,
                scheduledBlockNumber: thing.LastUpdatedAtBlockNumber!.Value + 5
            );
            task.SetPayload(new()
            {
                ["thingId"] = thing.Id!.Value.ToString(),
                ["data"] = Convert.ToBase64String(data)
            });
            _taskRepository.Create(task);
            thing.SetState(ThingState.VerifierLotteryInProgress);

            await _taskRepository.SaveChanges();
            await _thingRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}