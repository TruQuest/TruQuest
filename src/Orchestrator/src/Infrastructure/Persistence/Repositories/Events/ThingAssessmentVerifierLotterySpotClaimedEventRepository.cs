using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Nethereum.Util;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ThingAssessmentVerifierLotterySpotClaimedEventRepository : Repository<ThingAssessmentVerifierLotterySpotClaimedEvent>, IThingAssessmentVerifierLotterySpotClaimedEventRepository
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

    public Task<List<ThingAssessmentVerifierLotterySpotClaimedEvent>> FindAllFor(Guid thingId, Guid settlementProposalId)
    {
        var thingIdHash = Sha3Keccack.Current.CalculateHash(thingId.ToString());
        var settlementProposalIdHash = Sha3Keccack.Current.CalculateHash(settlementProposalId.ToString());

        return _dbContext.ThingAssessmentVerifierLotterySpotClaimedEvents
            .AsNoTracking()
            .Where(e => e.ThingIdHash == thingIdHash && e.SettlementProposalIdHash == settlementProposalIdHash)
            .OrderBy(e => e.BlockNumber)
                .ThenBy(e => e.TxnIndex)
            .ToListAsync();
    }
}