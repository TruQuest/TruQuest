using System.Security.Cryptography;

using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Commands.InitVerifierLottery;

public class InitVerifierLotteryCommand : IRequest<VoidResult>
{
    public Guid ThingId { get; init; }
    public Guid SettlementProposalId { get; init; }
}

internal class InitVerifierLotteryCommandHandler : IRequestHandler<InitVerifierLotteryCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractCaller _contractCaller;
    private readonly IContractStorageQueryable _contractStorageQueryable;
    private readonly IClientNotifier _clientNotifier;

    public InitVerifierLotteryCommandHandler(
        IThingRepository thingRepository,
        ISettlementProposalRepository settlementProposalRepository,
        ITaskRepository taskRepository,
        IContractCaller contractCaller,
        IContractStorageQueryable contractStorageQueryable,
        IClientNotifier clientNotifier
    )
    {
        _thingRepository = thingRepository;
        _settlementProposalRepository = settlementProposalRepository;
        _taskRepository = taskRepository;
        _contractCaller = contractCaller;
        _contractStorageQueryable = contractStorageQueryable;
        _clientNotifier = clientNotifier;
    }

    public async Task<VoidResult> Handle(InitVerifierLotteryCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        var proposal = await _settlementProposalRepository.FindById(command.SettlementProposalId);
        if (proposal.State == SettlementProposalState.AwaitingFunding)
        {
            var data = RandomNumberGenerator.GetBytes(32);
            var dataHash = await _contractCaller.ComputeHashForThingAssessmentVerifierLottery(data);

            long lotteryInitBlockNumber = await _contractCaller.InitThingAssessmentVerifierLottery(
                thing.Id.ToByteArray(), proposal.Id.ToByteArray(), dataHash
            );

            int lotteryDurationBlocks = await _contractStorageQueryable
                .GetThingAssessmentVerifierLotteryDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingAssessmentVerifierLottery,
                scheduledBlockNumber: lotteryInitBlockNumber + lotteryDurationBlocks
            );
            task.SetPayload(new()
            {
                ["thingId"] = thing.Id,
                ["settlementProposalId"] = proposal.Id,
                ["data"] = Convert.ToBase64String(data)
            });

            _taskRepository.Create(task);

            thing.SetState(ThingState.SettlementProposalFundedAndAssessmentVerifierLotteryInitiated);
            proposal.SetState(SettlementProposalState.FundedAndAssessmentVerifierLotteryInitiated);

            await _taskRepository.SaveChanges();
            await _thingRepository.SaveChanges();
            await _settlementProposalRepository.SaveChanges();

            await _clientNotifier.NotifySettlementProposalStateChanged(proposal.Id, proposal.State);
        }

        return VoidResult.Instance;
    }
}