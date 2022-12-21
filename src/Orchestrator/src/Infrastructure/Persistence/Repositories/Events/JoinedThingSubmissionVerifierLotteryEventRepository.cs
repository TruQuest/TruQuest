using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class JoinedThingSubmissionVerifierLotteryEventRepository : Repository<JoinedThingSubmissionVerifierLotteryEvent>, IJoinedThingSubmissionVerifierLotteryEventRepository
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

    public Task<List<JoinedThingSubmissionVerifierLotteryEvent>> FindWithClosestNonces(
        Guid thingId, long latestBlockNumber, decimal nonce, int count
    )
    {
        return _dbContext.JoinedThingSubmissionVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.BlockNumber <= latestBlockNumber)
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.BlockNumber)
                    .ThenBy(e => e.TxnIndex)
            .Take(count)
            .ToListAsync();
    }

    public Task<List<JoinedThingSubmissionVerifierLotteryEvent>> FindWithClosestNoncesAmongUsers(
        Guid thingId, IEnumerable<string> userIds, decimal nonce, int count
    )
    {
        return _dbContext.JoinedThingSubmissionVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && userIds.Contains(e.UserId))
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.BlockNumber)
                    .ThenBy(e => e.TxnIndex)
            .Take(count)
            .ToListAsync();
    }
}