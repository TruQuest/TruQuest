using System.Security.Cryptography;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using GoThataway;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Common.Misc;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Settlement.Commands.InitVerifierLottery;

[ExecuteInTxn]
public class InitVerifierLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(VoidResult _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId),
            (ActivityTags.SettlementProposalId, SettlementProposalId)
        };
    }
}

public class InitVerifierLotteryCommandHandler : IRequestHandler<InitVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<InitVerifierLotteryCommandHandler> _logger;
    private readonly IKeccakHasher _keccakHasher;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;
    private readonly IContractCaller _contractCaller;

    private readonly string _lotteryAddress;

    public InitVerifierLotteryCommandHandler(
        ILogger<InitVerifierLotteryCommandHandler> logger,
        IConfiguration configuration,
        IKeccakHasher keccakHasher,
        ISettlementProposalRepository settlementProposalRepository,
        ITaskRepository taskRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository,
        IContractCaller contractCaller
    )
    {
        _logger = logger;
        _keccakHasher = keccakHasher;
        _settlementProposalRepository = settlementProposalRepository;
        _taskRepository = taskRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
        _contractCaller = contractCaller;

        var network = configuration["Ethereum:Network"]!;
        _lotteryAddress = configuration[$"Ethereum:Contracts:{network}:SettlementProposalAssessmentVerifierLottery:Address"]!;
    }

    public async Task<VoidResult> Handle(InitVerifierLotteryCommand command, CancellationToken ct)
    {
        var state = await _settlementProposalRepository.GetStateFor(command.SettlementProposalId);
        if (state != SettlementProposalState.AwaitingFunding)
        {
            _logger.LogWarning(
                $"Trying to initialize an already initialized settlement proposal {SettlementProposalId} assessment verifier lottery",
                command.SettlementProposalId
            );
            return VoidResult.Instance;
        }

        var data = RandomNumberGenerator.GetBytes(32);
        var userXorData = RandomNumberGenerator.GetBytes(32);
        var dataHash = _keccakHasher.Hash(_lotteryAddress, data.ToHex(prefix: true));
        var userXorDataHash = _keccakHasher.Hash(_lotteryAddress, userXorData.ToHex(prefix: true));

        long lotteryInitBlockNumber = await _contractCaller.InitSettlementProposalAssessmentVerifierLottery(
            command.ThingId.ToByteArray(), command.SettlementProposalId.ToByteArray(),
            dataHash, userXorDataHash
        );

        int lotteryDurationBlocks = await _contractCaller.GetSettlementProposalAssessmentVerifierLotteryDurationBlocks();

        var task = new DeferredTask(
            type: TaskType.CloseSettlementProposalAssessmentVerifierLottery,
            scheduledBlockNumber: lotteryInitBlockNumber + lotteryDurationBlocks + 1
        );

        var payload = new Dictionary<string, object>()
        {
            ["thingId"] = command.ThingId,
            ["settlementProposalId"] = command.SettlementProposalId,
            ["data"] = data.ToHex(prefix: true),
            ["userXorData"] = userXorData.ToHex(prefix: true)
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        task.SetPayload(payload);

        _taskRepository.Create(task);

        await _settlementProposalRepository.UpdateStateFor(
            command.SettlementProposalId, SettlementProposalState.FundedAndVerifierLotteryInitiated
        );

        await _settlementProposalUpdateRepository.AddOrUpdate(new SettlementProposalUpdate(
            settlementProposalId: command.SettlementProposalId,
            category: SettlementProposalUpdateCategory.General,
            updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            title: "Proposal funded",
            details: "Verifier selection lottery initiated"
        ));

        await _taskRepository.SaveChanges();
        await _settlementProposalRepository.SaveChanges();
        await _settlementProposalUpdateRepository.SaveChanges();

        _logger.LogInformation(
            $"Initialized settlement proposal {SettlementProposalId} assessment verifier lottery.\n" +
            "Init block: {InitBlockNum}\n" +
            "End block: {EndBlockNum}",
            command.SettlementProposalId, lotteryInitBlockNumber, lotteryInitBlockNumber + lotteryDurationBlocks
        );

        return VoidResult.Instance;
    }
}
