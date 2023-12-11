using System.Diagnostics;

using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Results;
using Domain.Aggregates;

using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Thing.Commands.PrepareForValidationPoll;

[ExecuteInTxn]
public class PrepareForValidationPollCommand : IRequest<VoidResult>
{
    public required long InitBlockNumber { get; init; }
    public required int InitTxnIndex { get; init; }
    public required string InitTxnHash { get; init; }
    public required Guid ThingId { get; init; }
    public required string Orchestrator { get; init; }
    public required string Data { get; init; }
    public required string UserXorData { get; init; }
    public required string HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    public required List<string> WinnerWalletAddresses { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(VoidResult _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId)
        };
    }
}

public class PrepareForValidationPollCommandHandler : IRequestHandler<PrepareForValidationPollCommand, VoidResult>
{
    private readonly ILogger<PrepareForValidationPollCommandHandler> _logger;
    private readonly IThingRepository _thingRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;
    private readonly IContractCaller _contractCaller;

    public PrepareForValidationPollCommandHandler(
        ILogger<PrepareForValidationPollCommandHandler> logger,
        IThingRepository thingRepository,
        IUserRepository userRepository,
        ITaskRepository taskRepository,
        IThingUpdateRepository thingUpdateRepository,
        IWatchedItemRepository watchedItemRepository,
        IContractCaller contractCaller
    )
    {
        _logger = logger;
        _thingRepository = thingRepository;
        _userRepository = userRepository;
        _taskRepository = taskRepository;
        _thingUpdateRepository = thingUpdateRepository;
        _watchedItemRepository = watchedItemRepository;
        _contractCaller = contractCaller;
    }

    public async Task<VoidResult> Handle(PrepareForValidationPollCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        if (thing.State != ThingState.FundedAndVerifierLotteryInitiated)
        {
            _logger.LogWarning($"Trying to prepare an already prepared thing {ThingId} for validation poll", command.ThingId);
            return VoidResult.Instance;
        }

        thing.SetState(ThingState.VerifiersSelectedAndPollInitiated);
        var userIds = await _userRepository.GetUserIdsByWalletAddresses(command.WinnerWalletAddresses); // @@TODO: Use queryable.
        Debug.Assert(userIds.Count == command.WinnerWalletAddresses.Count);
        thing.AddVerifiers(userIds);

        var lotteryInitBlock = await _contractCaller.GetThingValidationVerifierLotteryInitBlock(thing.Id.ToByteArray());
        Debug.Assert(lotteryInitBlock < 0);

        var pollInitBlock = await _contractCaller.GetThingValidationPollInitBlock(thing.Id.ToByteArray());
        int pollDurationBlocks = await _contractCaller.GetThingValidationPollDurationBlocks();

        var task = new DeferredTask(
            type: TaskType.CloseThingValidationPoll,
            scheduledBlockNumber: pollInitBlock + pollDurationBlocks + 1
        );

        var payload = new Dictionary<string, object>()
        {
            ["thingId"] = thing.Id
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        task.SetPayload(payload);

        _taskRepository.Create(task);

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        _watchedItemRepository.Add(
            userIds
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
                details: "Validation poll initiated"
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
        await _watchedItemRepository.SaveChanges();
        await _thingUpdateRepository.SaveChanges();

        _logger.LogInformation($"Prepared thing {ThingId} for validation poll: added verifiers, created a closing task, etc", command.ThingId);

        return VoidResult.Instance;
    }
}
