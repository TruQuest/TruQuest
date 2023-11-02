using Domain.Aggregates.Events;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ThingValidationVerifierLotteryInitializedEventRepository :
    Repository,
    IThingValidationVerifierLotteryInitializedEventRepository
{
    private new readonly EventDbContext _dbContext;

    public ThingValidationVerifierLotteryInitializedEventRepository(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(ThingValidationVerifierLotteryInitializedEvent @event) =>
        _dbContext.ThingValidationVerifierLotteryInitializedEvents.Add(@event);
}
