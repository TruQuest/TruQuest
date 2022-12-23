using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class CastedAssessmentPollVoteEventRepository :
    Repository<CastedAssessmentPollVoteEvent>, ICastedAssessmentPollVoteEventRepository
{
    private readonly EventDbContext _dbContext;

    public CastedAssessmentPollVoteEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
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