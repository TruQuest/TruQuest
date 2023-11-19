using GoThataway;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;

namespace Application.Settlement.Commands.ArchiveDueToFailedLottery;

[ExecuteInTxn]
public class ArchiveDueToFailedLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
}

public class ArchiveDueToFailedLotteryCommandHandler : IRequestHandler<ArchiveDueToFailedLotteryCommand, VoidResult>
{
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public ArchiveDueToFailedLotteryCommandHandler(
        ISettlementProposalRepository settlementProposalRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository,
        IWatchedItemRepository watchedItemRepository
    )
    {
        _settlementProposalRepository = settlementProposalRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
        _watchedItemRepository = watchedItemRepository;
    }

    public async Task<VoidResult> Handle(ArchiveDueToFailedLotteryCommand command, CancellationToken ct)
    {
        var proposal = await _settlementProposalRepository.FindById(command.SettlementProposalId);
        if (proposal.State == SettlementProposalState.FundedAndVerifierLotteryInitiated)
        {
            proposal.SetState(SettlementProposalState.VerifierLotteryFailed);

            var proposalCopyId = await _settlementProposalRepository.DeepCopyFromWith(
                sourceProposalId: proposal.Id,
                state: SettlementProposalState.AwaitingFunding // @@??: Why 'AwaitingFunding' instead of 'Draft'? We want it to be editable, no?
            );

            proposal.AddRelatedProposalAs(proposalCopyId, relation: "next");

            await _settlementProposalUpdateRepository.AddOrUpdate(
                new SettlementProposalUpdate(
                    settlementProposalId: proposal.Id,
                    category: SettlementProposalUpdateCategory.General,
                    updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    title: "Verifier lottery failed",
                    details: "Not enough participants"
                )
            );

            await _watchedItemRepository.DuplicateGeneralItemsFrom(
                WatchedItemType.SettlementProposal,
                sourceItemId: proposal.Id,
                destItemId: proposalCopyId
            );

            await _settlementProposalRepository.SaveChanges();
            await _settlementProposalUpdateRepository.SaveChanges();
            await _watchedItemRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}
