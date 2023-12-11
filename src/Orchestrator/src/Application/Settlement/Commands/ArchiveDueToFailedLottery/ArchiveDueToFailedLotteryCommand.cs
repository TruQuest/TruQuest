using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Settlement.Commands.ArchiveDueToFailedLottery;

[ExecuteInTxn]
public class ArchiveDueToFailedLotteryCommand : IRequest<VoidResult>
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

public class ArchiveDueToFailedLotteryCommandHandler : IRequestHandler<ArchiveDueToFailedLotteryCommand, VoidResult>
{
    private readonly ILogger<ArchiveDueToFailedLotteryCommandHandler> _logger;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public ArchiveDueToFailedLotteryCommandHandler(
        ILogger<ArchiveDueToFailedLotteryCommandHandler> logger,
        ISettlementProposalRepository settlementProposalRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository,
        IWatchedItemRepository watchedItemRepository
    )
    {
        _logger = logger;
        _settlementProposalRepository = settlementProposalRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
        _watchedItemRepository = watchedItemRepository;
    }

    public async Task<VoidResult> Handle(ArchiveDueToFailedLotteryCommand command, CancellationToken ct)
    {
        var proposal = await _settlementProposalRepository.FindById(command.SettlementProposalId);
        if (proposal.State != SettlementProposalState.FundedAndVerifierLotteryInitiated)
        {
            _logger.LogWarning($"Trying to archive an already archived settlement proposal {SettlementProposalId}", proposal.Id);
            return VoidResult.Instance;
        }

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

        _logger.LogInformation(
            $"Created a new settlement proposal {SettlementProposalId} as a deep copy of existing proposal {RelatedSettlementProposalId}",
            proposalCopyId, proposal.Id
        );
        Telemetry.CurrentActivity?.SetTag(ActivityTags.RelatedSettlementProposalId, proposalCopyId);

        _logger.LogInformation($"Archived settlement proposal {SettlementProposalId} due to failed assessment verifier lottery", proposal.Id);

        return VoidResult.Instance;
    }
}
