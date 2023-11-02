using Domain.Aggregates.Events;

namespace Infrastructure.Persistence.Repositories.Events;

internal class SettlementProposalAssessmentVerifierLotteryInitializedEventRepository :
    Repository,
    ISettlementProposalAssessmentVerifierLotteryInitializedEventRepository
{
    private new readonly EventDbContext _dbContext;

    public SettlementProposalAssessmentVerifierLotteryInitializedEventRepository(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(SettlementProposalAssessmentVerifierLotteryInitializedEvent @event) =>
        _dbContext.SettlementProposalAssessmentVerifierLotteryInitializedEvents.Add(@event);
}
