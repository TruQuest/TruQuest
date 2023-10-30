using Microsoft.EntityFrameworkCore;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class CastedSettlementProposalAssessmentPollVoteEventRepository :
    Repository, ICastedSettlementProposalAssessmentPollVoteEventRepository
{
    private new readonly EventDbContext _dbContext;

    public CastedSettlementProposalAssessmentPollVoteEventRepository(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(CastedSettlementProposalAssessmentPollVoteEvent @event)
    {
        _dbContext.CastedSettlementProposalAssessmentPollVoteEvents.Add(@event);
    }

    public Task<List<CastedSettlementProposalAssessmentPollVoteEvent>> GetAllFor(Guid thingId, Guid proposalId)
    {
        return _dbContext.CastedSettlementProposalAssessmentPollVoteEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.SettlementProposalId == proposalId)
            .ToListAsync();
    }
}
