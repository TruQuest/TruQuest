using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Nethereum.Util;

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
        var thingIdHash = Sha3Keccack.Current.CalculateHash(thingId.ToString());
        return _dbContext.JoinedVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingIdHash == thingIdHash && e.BlockNumber <= latestBlockNumber)
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.BlockNumber)
                    .ThenBy(e => e.TxnIndex)
            .Take(count)
            .ToListAsync();
    }
}