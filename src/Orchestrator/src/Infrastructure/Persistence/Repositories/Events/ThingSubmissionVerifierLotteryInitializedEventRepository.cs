using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ThingSubmissionVerifierLotteryInitializedEventRepository :
    Repository,
    IThingSubmissionVerifierLotteryInitializedEventRepository
{
    private new readonly EventDbContext _dbContext;

    public ThingSubmissionVerifierLotteryInitializedEventRepository(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(ThingSubmissionVerifierLotteryInitializedEvent @event) =>
        _dbContext.ThingSubmissionVerifierLotteryInitializedEvents.Add(@event);
}
