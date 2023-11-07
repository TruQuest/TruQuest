using Microsoft.EntityFrameworkCore;

using Domain.Aggregates.Events;

namespace Infrastructure.Persistence.Repositories.Events;

internal class CastedSettlementProposalAssessmentPollVoteEventRepository :
    Repository, ICastedSettlementProposalAssessmentPollVoteEventRepository
{
    private new readonly EventDbContext _dbContext;

    public CastedSettlementProposalAssessmentPollVoteEventRepository(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(CastedSettlementProposalAssessmentPollVoteEvent @event) =>
        _dbContext.CastedSettlementProposalAssessmentPollVoteEvents.Add(@event);

    public Task<List<CastedSettlementProposalAssessmentPollVoteEvent>> GetAllFor(Guid thingId, Guid proposalId)
    {
        return _dbContext.CastedSettlementProposalAssessmentPollVoteEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.SettlementProposalId == proposalId)
            .ToListAsync();
    }
}
