using System.Security.Cryptography;

using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Attributes;

namespace Application.Thing.Commands.InitVerifierLottery;

[ExecuteInTxn]
public class InitVerifierLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
}

internal class InitVerifierLotteryCommandHandler : IRequestHandler<InitVerifierLotteryCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractCaller _contractCaller;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public InitVerifierLotteryCommandHandler(
        IThingRepository thingRepository,
        IThingUpdateRepository thingUpdateRepository,
        ITaskRepository taskRepository,
        IContractCaller contractCaller,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _thingRepository = thingRepository;
        _thingUpdateRepository = thingUpdateRepository;
        _taskRepository = taskRepository;
        _contractCaller = contractCaller;
        _contractStorageQueryable = contractStorageQueryable;
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

            thing.SetState(ThingState.FundedAndVerifierLotteryInitiated);

            await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                thingId: thing.Id,
                category: ThingUpdateCategory.General,
                updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                title: "Thing updated!!!",
                details: "Some details"
            ));

            await _taskRepository.SaveChanges();
            await _thingRepository.SaveChanges();
            await _thingUpdateRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}