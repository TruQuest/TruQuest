using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Nethereum.Util;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class JoinedLotteryEventRepository : Repository<JoinedLotteryEvent>, IJoinedLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public JoinedLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(JoinedLotteryEvent @event)
    {
        _dbContext.JoinedLotteryEvents.Add(@event);
    }

    public Task<List<JoinedLotteryEvent>> FindWithClosestNonces(
        Guid thingId, long latestBlockNumber, decimal nonce, int count
    )
    {
        var thingIdHash = Sha3Keccack.Current.CalculateHash(thingId.ToString());
        return _dbContext.JoinedLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingIdHash == thingIdHash && e.BlockNumber <= latestBlockNumber)
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.BlockNumber)
                    .ThenBy(e => e.TxnIndex)
            .Take(count)
            .ToListAsync();
    }
}