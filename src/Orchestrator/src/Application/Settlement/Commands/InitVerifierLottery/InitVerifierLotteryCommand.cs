using System.Security.Cryptography;

using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Common.Misc;

namespace Application.Settlement.Commands.InitVerifierLottery;

[ExecuteInTxn]
public class InitVerifierLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
}

internal class InitVerifierLotteryCommandHandler : IRequestHandler<InitVerifierLotteryCommand, VoidResult>
{
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;
    private readonly IContractCaller _contractCaller;

    public InitVerifierLotteryCommandHandler(
        ISettlementProposalRepository settlementProposalRepository,
        ITaskRepository taskRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository,
        IContractCaller contractCaller
    )
    {
        _settlementProposalRepository = settlementProposalRepository;
        _taskRepository = taskRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
        _contractCaller = contractCaller;
    }

    public async Task<VoidResult> Handle(InitVerifierLotteryCommand command, CancellationToken ct)
    {
        var state = await _settlementProposalRepository.GetStateFor(command.SettlementProposalId);
        if (state == SettlementProposalState.AwaitingFunding)
        {
            var data = RandomNumberGenerator.GetBytes(32);
            var userXorData = RandomNumberGenerator.GetBytes(32);
            // @@TODO!!: Compute on the server!
            var dataHash = await _contractCaller.ComputeHashForThingAssessmentVerifierLottery(data);
            var userXorDataHash = await _contractCaller.ComputeHashForThingAssessmentVerifierLottery(userXorData);

            long lotteryInitBlockNumber = await _contractCaller.InitThingAssessmentVerifierLottery(
                command.ThingId.ToByteArray(), command.SettlementProposalId.ToByteArray(),
                dataHash, userXorDataHash
            );

            int lotteryDurationBlocks = await _contractCaller.GetThingAssessmentVerifierLotteryDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingAssessmentVerifierLottery,
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
        }

        return VoidResult.Instance;
    }
}
