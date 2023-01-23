using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class JoinedThingSubmissionVerifierLotteryEventRepository :
    Repository<JoinedThingSubmissionVerifierLotteryEvent>, IJoinedThingSubmissionVerifierLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public JoinedThingSubmissionVerifierLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(JoinedThingSubmissionVerifierLotteryEvent @event)
    {
        _dbContext.JoinedThingSubmissionVerifierLotteryEvents.Add(@event);
    }
}