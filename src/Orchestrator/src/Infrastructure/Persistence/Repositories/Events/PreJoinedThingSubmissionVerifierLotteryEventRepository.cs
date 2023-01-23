using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class PreJoinedThingSubmissionVerifierLotteryEventRepository :
    Repository<PreJoinedThingSubmissionVerifierLotteryEvent>, IPreJoinedThingSubmissionVerifierLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public PreJoinedThingSubmissionVerifierLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(PreJoinedThingSubmissionVerifierLotteryEvent @event)
    {
        _dbContext.PreJoinedThingSubmissionVerifierLotteryEvents.Add(@event);
    }
}