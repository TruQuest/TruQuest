using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Commands.PrepareForAssessmentPoll;

public class PrepareForAssessmentPollCommand : IRequest<VoidResult>
{
    public long AssessmentPollInitBlockNumber { get; init; }
    public Guid ThingId { get; init; }
    public Guid SettlementProposalId { get; init; }
    public required List<string> ClaimantIds { get; init; }
    public required List<string> WinnerIds { get; init; }
}

internal class PrepareForAssessmentPollCommandHandler : IRequestHandler<PrepareForAssessmentPollCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public PrepareForAssessmentPollCommandHandler(
        IThingRepository thingRepository,
        ISettlementProposalRepository settlementProposalRepository,
        ITaskRepository taskRepository,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _thingRepository = thingRepository;
        _settlementProposalRepository = settlementProposalRepository;
        _taskRepository = taskRepository;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(PrepareForAssessmentPollCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        var proposal = await _settlementProposalRepository.FindById(command.SettlementProposalId);
        if (proposal.State == SettlementProposalState.FundedAndAssessmentVerifierLotteryInitiated)
        {
            thing.SetState(ThingState.SettlementProposalAssessmentVerifiersSelectedAndPollInitiated);
            proposal.SetState(SettlementProposalState.AssessmentVerifiersSelectedAndPollInitiated);
            proposal.AddVerifiers(command.ClaimantIds.Concat(command.WinnerIds));

            int pollDurationBlocks = await _contractStorageQueryable.GetAssessmentPollDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingSettlementProposalAssessmentPoll,
                scheduledBlockNumber: command.AssessmentPollInitBlockNumber + pollDurationBlocks
            );
            task.SetPayload(new()
            {
                ["thingId"] = command.ThingId,
                ["settlementProposalId"] = proposal.Id!
            });
            _taskRepository.Create(task);

            await _thingRepository.SaveChanges();
            await _settlementProposalRepository.SaveChanges();
            await _taskRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}