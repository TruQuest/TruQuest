using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class PreJoinedThingAssessmentVerifierLotteryEventRepository :
    Repository<PreJoinedThingAssessmentVerifierLotteryEvent>,
    IPreJoinedThingAssessmentVerifierLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public PreJoinedThingAssessmentVerifierLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(PreJoinedThingAssessmentVerifierLotteryEvent @event)
    {
        _dbContext.PreJoinedThingAssessmentVerifierLotteryEvents.Add(@event);
    }
}