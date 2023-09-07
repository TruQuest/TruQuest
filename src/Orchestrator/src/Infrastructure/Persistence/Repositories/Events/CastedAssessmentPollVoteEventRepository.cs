using Microsoft.EntityFrameworkCore;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class CastedAssessmentPollVoteEventRepository : Repository, ICastedAssessmentPollVoteEventRepository
{
    private new readonly EventDbContext _dbContext;

    public CastedAssessmentPollVoteEventRepository(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(CastedAssessmentPollVoteEvent @event)
    {
        _dbContext.CastedAssessmentPollVoteEvents.Add(@event);
    }

    public Task<List<CastedAssessmentPollVoteEvent>> GetAllFor(Guid thingId, Guid settlementProposalId)
    {
        return _dbContext.CastedAssessmentPollVoteEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.SettlementProposalId == settlementProposalId)
            .ToListAsync();
    }
}
