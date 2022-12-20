using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Nethereum.Util;

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

    public Task<List<JoinedThingAssessmentVerifierLotteryEvent>> FindWithClosestNonces(
        Guid thingId, Guid settlementProposalId, long latestBlockNumber, decimal nonce, int count
    )
    {
        var thingIdHash = Sha3Keccack.Current.CalculateHash(thingId.ToString());
        var settlementProposalIdHash = Sha3Keccack.Current.CalculateHash(settlementProposalId.ToString());

        return _dbContext.JoinedThingAssessmentVerifierLotteryEvents
            .AsNoTracking()
            .Where(e =>
                e.ThingIdHash == thingIdHash &&
                e.SettlementProposalIdHash == settlementProposalIdHash &&
                e.BlockNumber <= latestBlockNumber
            )
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.BlockNumber)
                    .ThenBy(e => e.TxnIndex)
            .Take(count)
            .ToListAsync();
    }
}