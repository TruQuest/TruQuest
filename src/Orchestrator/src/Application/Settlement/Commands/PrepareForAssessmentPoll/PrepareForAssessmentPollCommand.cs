using MediatR;

using Domain.Aggregates;
using Domain.Results;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Common.Attributes;

namespace Application.Settlement.Commands.PrepareForAssessmentPoll;

[ExecuteInTxn]
public class PrepareForAssessmentPollCommand : IRequest<VoidResult>
{
    public required long AssessmentPollInitBlockNumber { get; init; }
    public required int AssessmentPollInitTxnIndex { get; init; }
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required string Orchestrator { get; init; }
    public required decimal Nonce { get; init; }
    public required List<string> ClaimantIds { get; init; }
    public required List<string> WinnerIds { get; init; }
}

internal class PrepareForAssessmentPollCommandHandler : IRequestHandler<PrepareForAssessmentPollCommand, VoidResult>
{
    private readonly IThingRepository _thingRepository;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IJoinedThingAssessmentVerifierLotteryEventRepository _joinedThingAssessmentVerifierLotteryEventRepository;
    private readonly IContractStorageQueryable _contractStorageQueryable;
    private readonly IClientNotifier _clientNotifier;

    public PrepareForAssessmentPollCommandHandler(
        IThingRepository thingRepository,
        ISettlementProposalRepository settlementProposalRepository,
        ITaskRepository taskRepository,
        IJoinedThingAssessmentVerifierLotteryEventRepository joinedThingAssessmentVerifierLotteryEventRepository,
        IContractStorageQueryable contractStorageQueryable,
        IClientNotifier clientNotifier
    )
    {
        _thingRepository = thingRepository;
        _settlementProposalRepository = settlementProposalRepository;
        _taskRepository = taskRepository;
        _joinedThingAssessmentVerifierLotteryEventRepository = joinedThingAssessmentVerifierLotteryEventRepository;
        _contractStorageQueryable = contractStorageQueryable;
        _clientNotifier = clientNotifier;
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

            var joinedThingAssessmentVerifierLotteryEvent = new JoinedThingAssessmentVerifierLotteryEvent(
                blockNumber: command.AssessmentPollInitBlockNumber,
                txnIndex: command.AssessmentPollInitTxnIndex,
                thingId: command.ThingId,
                settlementProposalId: command.SettlementProposalId,
                userId: command.Orchestrator,
                nonce: command.Nonce
            );
            _joinedThingAssessmentVerifierLotteryEventRepository.Create(joinedThingAssessmentVerifierLotteryEvent);

            await _thingRepository.SaveChanges();
            await _settlementProposalRepository.SaveChanges();
            await _taskRepository.SaveChanges();
            await _joinedThingAssessmentVerifierLotteryEventRepository.SaveChanges();

            await _clientNotifier.NotifySettlementProposalStateChanged(proposal.Id, proposal.State);
        }

        return VoidResult.Instance;
    }
}