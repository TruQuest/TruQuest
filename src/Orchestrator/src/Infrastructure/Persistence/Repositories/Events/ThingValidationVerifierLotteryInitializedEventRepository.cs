using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ThingValidationVerifierLotteryInitializedEventRepository :
    Repository,
    IThingValidationVerifierLotteryInitializedEventRepository
{
    private new readonly EventDbContext _dbContext;

    public ThingValidationVerifierLotteryInitializedEventRepository(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(ThingValidationVerifierLotteryInitializedEvent @event) =>
        _dbContext.ThingValidationVerifierLotteryInitializedEvents.Add(@event);
}
