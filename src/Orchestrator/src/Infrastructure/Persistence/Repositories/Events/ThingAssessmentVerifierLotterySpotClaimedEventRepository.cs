using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ThingAssessmentVerifierLotterySpotClaimedEventRepository :
    Repository<ThingAssessmentVerifierLotterySpotClaimedEvent>,
    IThingAssessmentVerifierLotterySpotClaimedEventRepository
{
    private readonly EventDbContext _dbContext;

    public ThingAssessmentVerifierLotterySpotClaimedEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(ThingAssessmentVerifierLotterySpotClaimedEvent @event)
    {
        _dbContext.ThingAssessmentVerifierLotterySpotClaimedEvents.Add(@event);
    }
}