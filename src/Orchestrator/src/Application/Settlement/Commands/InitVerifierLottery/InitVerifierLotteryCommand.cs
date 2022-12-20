using System.Security.Cryptography;

using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Commands.InitVerifierLottery;

public class InitVerifierLotteryCommand : IRequest<VoidResult>
{
    public required string ThingIdHash { get; init; }
    public required string SettlementProposalIdHash { get; init; }
}

internal class InitVerifierLotteryCommandHandler : IRequestHandler<InitVerifierLotteryCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractCaller _contractCaller;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public InitVerifierLotteryCommandHandler(
        IThingRepository thingRepository,
        ISettlementProposalRepository settlementProposalRepository,
        ITaskRepository taskRepository,
        IContractCaller contractCaller,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _thingRepository = thingRepository;
        _settlementProposalRepository = settlementProposalRepository;
        _taskRepository = taskRepository;
        _contractCaller = contractCaller;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(InitVerifierLotteryCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindByIdHash(command.ThingIdHash);
        var proposal = await _settlementProposalRepository.FindByIdHash(command.SettlementProposalIdHash);

        var data = RandomNumberGenerator.GetBytes(32);
        var dataHash = await _contractCaller.ComputeHashForThingAssessmentVerifierLottery(data);

        long lotteryInitBlockNumber = await _contractCaller.InitThingAssessmentVerifierLottery(
            thing.Id!.Value.ToString(), dataHash
        );

        int lotteryDurationBlocks = await _contractStorageQueryable.GetThingAssessmentVerifierLotteryDurationBlocks();

        var task = new DeferredTask(
            type: TaskType.CloseThingAssessmentVerifierLottery,
            scheduledBlockNumber: lotteryInitBlockNumber + lotteryDurationBlocks
        );
        task.SetPayload(new()
        {
            ["thingId"] = thing.Id!.Value.ToString(),
            ["settlementProposalId"] = proposal.Id!.Value.ToString(),
            ["data"] = Convert.ToBase64String(data)
        });

        _taskRepository.Create(task);

        thing.SetState(ThingState.AssessmentInProgress);
        proposal.SetState(SettlementProposalState.FundedAndVerifierLotteryInitiated);

        await _taskRepository.SaveChanges();
        await _thingRepository.SaveChanges();
        await _settlementProposalRepository.SaveChanges();

        return VoidResult.Instance;
    }
}