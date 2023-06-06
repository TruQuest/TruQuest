using System.Security.Cryptography;

using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Common.Misc;

namespace Application.Thing.Commands.InitVerifierLottery;

[ExecuteInTxn]
public class InitVerifierLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
}

internal class InitVerifierLotteryCommandHandler : IRequestHandler<InitVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<InitVerifierLotteryCommandHandler> _logger;
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractCaller _contractCaller;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public InitVerifierLotteryCommandHandler(
        ILogger<InitVerifierLotteryCommandHandler> logger,
        IThingRepository thingRepository,
        IThingUpdateRepository thingUpdateRepository,
        ITaskRepository taskRepository,
        IContractCaller contractCaller,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _logger = logger;
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
            var userXorData = RandomNumberGenerator.GetBytes(32);
            var dataHash = await _contractCaller.ComputeHashForThingSubmissionVerifierLottery(data);
            var userXorDataHash = await _contractCaller.ComputeHashForThingSubmissionVerifierLottery(userXorData);

            long lotteryInitBlockNumber = await _contractCaller.InitThingSubmissionVerifierLottery(
                thing.Id.ToByteArray(), dataHash, userXorDataHash
            );

            _logger.LogInformation("Thing {ThingId} Lottery Init Block: {BlockNum}", thing.Id, lotteryInitBlockNumber);

            int lotteryDurationBlocks = await _contractStorageQueryable.GetThingSubmissionVerifierLotteryDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingSubmissionVerifierLottery,
                scheduledBlockNumber: lotteryInitBlockNumber + lotteryDurationBlocks + 1
            );
            task.SetPayload(new()
            {
                ["thingId"] = thing.Id!,
                ["data"] = data.ToHex(prefix: true),
                ["userXorData"] = userXorData.ToHex(prefix: true)
            });

            _taskRepository.Create(task);

            thing.SetState(ThingState.FundedAndVerifierLotteryInitiated);

            await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                thingId: thing.Id,
                category: ThingUpdateCategory.General,
                updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                title: "Promise funded",
                details: "Verifier selection lottery initiated"
            ));

            await _taskRepository.SaveChanges();
            await _thingRepository.SaveChanges();
            await _thingUpdateRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}