using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class JoinedVerifierLotteryEventRepository : Repository<JoinedVerifierLotteryEvent>, IJoinedVerifierLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public JoinedVerifierLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(JoinedVerifierLotteryEvent @event)
    {
        _dbContext.JoinedVerifierLotteryEvents.Add(@event);
    }

    public Task<List<JoinedVerifierLotteryEvent>> FindWithClosestNonces(
        Guid thingId, long latestBlockNumber, decimal nonce, int count
    )
    {
        return _dbContext.JoinedVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.BlockNumber <= latestBlockNumber)
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.BlockNumber)
                    .ThenBy(e => e.TxnIndex)
            .Take(count)
            .ToListAsync();
    }

    public Task<List<JoinedVerifierLotteryEvent>> FindWithClosestNoncesAmongUsers(
        Guid thingId, IEnumerable<string> userIds, decimal nonce, int count
    )
    {
        return _dbContext.JoinedVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && userIds.Contains(e.UserId))
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.BlockNumber)
                    .ThenBy(e => e.TxnIndex)
            .Take(count)
            .ToListAsync();
    }
}