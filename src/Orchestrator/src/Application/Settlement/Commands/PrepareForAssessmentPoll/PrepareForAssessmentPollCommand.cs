using System.Diagnostics;

using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Attributes;

namespace Application.Settlement.Commands.PrepareForAssessmentPoll;

[ExecuteInTxn]
public class PrepareForAssessmentPollCommand : IRequest<VoidResult>
{
    public required long AssessmentPollInitBlockNumber { get; init; }
    public required int AssessmentPollInitTxnIndex { get; init; }
    public required string AssessmentPollInitTxnHash { get; init; }
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required string Orchestrator { get; init; }
    public required string Data { get; init; }
    public required string UserXorData { get; init; }
    public required string HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    public required List<string> ClaimantWalletAddresses { get; init; }
    public required List<string> WinnerWalletAddresses { get; init; }
}

internal class PrepareForAssessmentPollCommandHandler : IRequestHandler<PrepareForAssessmentPollCommand, VoidResult>
{
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;
    private readonly IContractCaller _contractCaller;

    public PrepareForAssessmentPollCommandHandler(
        ISettlementProposalRepository settlementProposalRepository,
        IUserRepository userRepository,
        ITaskRepository taskRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository,
        IWatchedItemRepository watchedItemRepository,
        IContractCaller contractCaller
    )
    {
        _settlementProposalRepository = settlementProposalRepository;
        _userRepository = userRepository;
        _taskRepository = taskRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
        _watchedItemRepository = watchedItemRepository;
        _contractCaller = contractCaller;
    }

    public async Task<VoidResult> Handle(PrepareForAssessmentPollCommand command, CancellationToken ct)
    {
        var proposal = await _settlementProposalRepository.FindById(command.SettlementProposalId);
        if (proposal.State == SettlementProposalState.FundedAndVerifierLotteryInitiated)
        {
            proposal.SetState(SettlementProposalState.VerifiersSelectedAndPollInitiated);
            var userIds = await _userRepository.GetUserIdsByWalletAddresses(
                command.ClaimantWalletAddresses.Concat(command.WinnerWalletAddresses)
            );
            Debug.Assert(userIds.Count == command.ClaimantWalletAddresses.Count + command.WinnerWalletAddresses.Count);
            proposal.AddVerifiers(userIds);

            var lotteryInitBlock = await _contractCaller.GetThingAssessmentVerifierLotteryInitBlock(
                proposal.ThingId.ToByteArray(), proposal.Id.ToByteArray()
            );
            Debug.Assert(lotteryInitBlock < 0);

            var pollInitBlock = await _contractCaller.GetThingAssessmentPollInitBlock(
                proposal.ThingId.ToByteArray(), proposal.Id.ToByteArray()
            );
            int pollDurationBlocks = await _contractCaller.GetThingAssessmentPollDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingSettlementProposalAssessmentPoll,
                scheduledBlockNumber: pollInitBlock + pollDurationBlocks + 1
            );

            var payload = new Dictionary<string, object>()
            {
                ["thingId"] = proposal.ThingId,
                ["settlementProposalId"] = proposal.Id
            };

            Telemetry.CurrentActivity!.AddTraceparentTo(payload);
            task.SetPayload(payload);

            _taskRepository.Create(task);

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            _watchedItemRepository.Add(
                userIds
                    .Select(userId => new WatchedItem(
                        userId: userId,
                        itemType: WatchedItemType.SettlementProposal,
                        itemId: proposal.Id,
                        itemUpdateCategory: (int)SettlementProposalUpdateCategory.Special,
                        lastSeenUpdateTimestamp: now
                    ))
                    .ToArray()
            );

            await _settlementProposalUpdateRepository.AddOrUpdate(
                new SettlementProposalUpdate(
                    settlementProposalId: proposal.Id,
                    category: SettlementProposalUpdateCategory.General,
                    updateTimestamp: now + 10,
                    title: "Verifier lottery completed",
                    details: "Assessment poll initiated"
                ),
                new SettlementProposalUpdate(
                    settlementProposalId: proposal.Id,
                    category: SettlementProposalUpdateCategory.Special,
                    updateTimestamp: now + 10,
                    title: "You've been selected as a verifier!",
                    details: null
                )
            );

            await _settlementProposalRepository.SaveChanges();
            await _taskRepository.SaveChanges();
            await _watchedItemRepository.SaveChanges();
            await _settlementProposalUpdateRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}
