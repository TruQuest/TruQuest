using System.Security.Cryptography;

using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Common.Misc;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Thing.Commands.InitVerifierLottery;

[ExecuteInTxn]
public class InitVerifierLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(VoidResult _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId)
        };
    }
}

public class InitVerifierLotteryCommandHandler : IRequestHandler<InitVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<InitVerifierLotteryCommandHandler> _logger;
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractCaller _contractCaller;

    public InitVerifierLotteryCommandHandler(
        ILogger<InitVerifierLotteryCommandHandler> logger,
        IThingRepository thingRepository,
        IThingUpdateRepository thingUpdateRepository,
        ITaskRepository taskRepository,
        IContractCaller contractCaller
    )
    {
        _logger = logger;
        _thingRepository = thingRepository;
        _thingUpdateRepository = thingUpdateRepository;
        _taskRepository = taskRepository;
        _contractCaller = contractCaller;
    }

    public async Task<VoidResult> Handle(InitVerifierLotteryCommand command, CancellationToken ct)
    {
        var state = await _thingRepository.GetStateFor(command.ThingId);
        if (state != ThingState.AwaitingFunding)
        {
            _logger.LogWarning($"Trying to initialize an already initialized thing {ThingId} validation verifier lottery", command.ThingId);
            return VoidResult.Instance;
        }

        var data = RandomNumberGenerator.GetBytes(32);
        var userXorData = RandomNumberGenerator.GetBytes(32);
        // @@TODO!!: Compute on the server!
        var dataHash = await _contractCaller.ComputeHashForThingValidationVerifierLottery(data);
        var userXorDataHash = await _contractCaller.ComputeHashForThingValidationVerifierLottery(userXorData);

        long lotteryInitBlockNumber = await _contractCaller.InitThingValidationVerifierLottery(
            command.ThingId.ToByteArray(), dataHash, userXorDataHash
        );

        // @@NOTE: In a theoretical situation when something glitches and two identical Init commands get dispatched one
        // after another, even though the above call will fail for the second command, we still should continue executing
        // the code below, since we don't know which transaction Postgres will keep and which one it will discard due to
        // serialization failure.

        int lotteryDurationBlocks = await _contractCaller.GetThingValidationVerifierLotteryDurationBlocks();

        var task = new DeferredTask(
            type: TaskType.CloseThingValidationVerifierLottery,
            scheduledBlockNumber: lotteryInitBlockNumber + lotteryDurationBlocks + 1
        );
        var payload = new Dictionary<string, object>()
        {
            ["thingId"] = command.ThingId,
            ["data"] = data.ToHex(prefix: true),
            ["userXorData"] = userXorData.ToHex(prefix: true)
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        task.SetPayload(payload);

        _taskRepository.Create(task);

        await _thingRepository.UpdateStateFor(command.ThingId, ThingState.FundedAndVerifierLotteryInitiated);

        await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
            thingId: command.ThingId,
            category: ThingUpdateCategory.General,
            updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            title: "Promise funded",
            details: "Verifier selection lottery initiated"
        ));

        await _taskRepository.SaveChanges();
        await _thingRepository.SaveChanges();
        await _thingUpdateRepository.SaveChanges();

        _logger.LogInformation(
            $"Initialized thing {ThingId} validation verifier lottery.\nInit block: {InitBlockNum}\nEnd block: {EndBlockNum}",
            command.ThingId, lotteryInitBlockNumber, lotteryInitBlockNumber + lotteryDurationBlocks
        );

        return VoidResult.Instance;
    }
}
