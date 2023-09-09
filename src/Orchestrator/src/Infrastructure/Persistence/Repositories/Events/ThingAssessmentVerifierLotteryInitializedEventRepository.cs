using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ThingAssessmentVerifierLotteryInitializedEventRepository :
    Repository,
    IThingAssessmentVerifierLotteryInitializedEventRepository
{
    private new readonly EventDbContext _dbContext;

    public ThingAssessmentVerifierLotteryInitializedEventRepository(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(ThingAssessmentVerifierLotteryInitializedEvent @event) =>
        _dbContext.ThingAssessmentVerifierLotteryInitializedEvents.Add(@event);
}
