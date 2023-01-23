using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class JoinedThingAssessmentVerifierLotteryEventRepository :
    Repository<JoinedThingAssessmentVerifierLotteryEvent>,
    IJoinedThingAssessmentVerifierLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public JoinedThingAssessmentVerifierLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(JoinedThingAssessmentVerifierLotteryEvent @event)
    {
        _dbContext.JoinedThingAssessmentVerifierLotteryEvents.Add(@event);
    }
}