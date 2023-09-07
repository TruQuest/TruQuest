using Microsoft.EntityFrameworkCore;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class SettlementProposalAssessmentVerifierLotteryEventQueryable :
    Queryable, ISettlementProposalAssessmentVerifierLotteryEventQueryable
{
    private new readonly EventDbContext _dbContext;

    public SettlementProposalAssessmentVerifierLotteryEventQueryable(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public Task<List<ThingAssessmentVerifierLotterySpotClaimedEvent>> GetAllSpotClaimedEventsFor(
        Guid thingId, Guid settlementProposalId
    )
    {
        return _dbContext.ThingAssessmentVerifierLotterySpotClaimedEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.SettlementProposalId == settlementProposalId)
            .OrderBy(e => e.BlockNumber)
                .ThenBy(e => e.TxnIndex)
            .ToListAsync();
    }
}
