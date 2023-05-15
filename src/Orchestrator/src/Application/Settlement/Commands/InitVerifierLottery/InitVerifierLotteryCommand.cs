using System.Security.Cryptography;

using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Attributes;

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
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public InitVerifierLotteryCommandHandler(
        ISettlementProposalRepository settlementProposalRepository,
        ITaskRepository taskRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository,
        IContractCaller contractCaller,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _settlementProposalRepository = settlementProposalRepository;
        _taskRepository = taskRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
        _contractCaller = contractCaller;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(InitVerifierLotteryCommand command, CancellationToken ct)
    {
        var proposal = await _settlementProposalRepository.FindById(command.SettlementProposalId);
        if (proposal.State == SettlementProposalState.AwaitingFunding)
        {
            var data = RandomNumberGenerator.GetBytes(32);
            var dataHash = await _contractCaller.ComputeHashForThingAssessmentVerifierLottery(data);

            long lotteryInitBlockNumber = await _contractCaller.InitThingAssessmentVerifierLottery(
                command.ThingId.ToByteArray(), command.SettlementProposalId.ToByteArray(), dataHash
            );

            int lotteryDurationBlocks = await _contractStorageQueryable
                .GetThingAssessmentVerifierLotteryDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingAssessmentVerifierLottery,
                scheduledBlockNumber: lotteryInitBlockNumber + lotteryDurationBlocks
            );
            task.SetPayload(new()
            {
                ["thingId"] = command.ThingId,
                ["settlementProposalId"] = proposal.Id,
                ["data"] = Convert.ToBase64String(data)
            });

            _taskRepository.Create(task);

            proposal.SetState(SettlementProposalState.FundedAndVerifierLotteryInitiated);

            await _settlementProposalUpdateRepository.AddOrUpdate(new SettlementProposalUpdate(
                settlementProposalId: proposal.Id,
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