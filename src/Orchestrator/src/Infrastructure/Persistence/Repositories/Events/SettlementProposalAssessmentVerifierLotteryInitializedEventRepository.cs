using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class SettlementProposalAssessmentVerifierLotteryInitializedEventRepository :
    Repository,
    ISettlementProposalAssessmentVerifierLotteryInitializedEventRepository
{
    private new readonly EventDbContext _dbContext;

    public SettlementProposalAssessmentVerifierLotteryInitializedEventRepository(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(SettlementProposalAssessmentVerifierLotteryInitializedEvent @event) =>
        _dbContext.SettlementProposalAssessmentVerifierLotteryInitializedEvents.Add(@event);
}
