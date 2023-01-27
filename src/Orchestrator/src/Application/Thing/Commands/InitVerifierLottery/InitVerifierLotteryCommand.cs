using System.Security.Cryptography;

using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Thing.Commands.InitVerifierLottery;

public class InitVerifierLotteryCommand : IRequest<VoidResult>
{
    public Guid ThingId { get; init; }
}

internal class InitVerifierLotteryCommandHandler : IRequestHandler<InitVerifierLotteryCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractCaller _contractCaller;
    private readonly IContractStorageQueryable _contractStorageQueryable;
    private readonly IClientNotifier _clientNotifier;

    public InitVerifierLotteryCommandHandler(
        IThingRepository thingRepository,
        ITaskRepository taskRepository,
        IContractCaller contractCaller,
        IContractStorageQueryable contractStorageQueryable,
        IClientNotifier clientNotifier
    )
    {
        _thingRepository = thingRepository;
        _taskRepository = taskRepository;
        _contractCaller = contractCaller;
        _contractStorageQueryable = contractStorageQueryable;
        _clientNotifier = clientNotifier;
    }

    public async Task<VoidResult> Handle(InitVerifierLotteryCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        if (thing.State == ThingState.AwaitingFunding)
        {
            var data = RandomNumberGenerator.GetBytes(32);
            var dataHash = await _contractCaller.ComputeHashForThingSubmissionVerifierLottery(data);

            long lotteryInitBlockNumber = await _contractCaller.InitThingSubmissionVerifierLottery(
                thing.Id.ToByteArray(), dataHash
            );

            int lotteryDurationBlocks = await _contractStorageQueryable.GetThingSubmissionVerifierLotteryDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingSubmissionVerifierLottery,
                scheduledBlockNumber: lotteryInitBlockNumber + lotteryDurationBlocks
            );
            task.SetPayload(new()
            {
                ["thingId"] = thing.Id!,
                ["data"] = Convert.ToBase64String(data)
            });

            _taskRepository.Create(task);

            thing.SetState(ThingState.FundedAndSubmissionVerifierLotteryInitiated);

            await _taskRepository.SaveChanges();
            await _thingRepository.SaveChanges();

            await _clientNotifier.NotifyThingStateChanged(thing.Id, thing.State);
        }

        return VoidResult.Instance;
    }
}