using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class CastedAcceptancePollVoteEventRepository :
    Repository<CastedAcceptancePollVoteEvent>, ICastedAcceptancePollVoteEventRepository
{
    private readonly EventDbContext _dbContext;

    public CastedAcceptancePollVoteEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(CastedAcceptancePollVoteEvent @event)
    {
        _dbContext.CastedAcceptancePollVoteEvents.Add(@event);
    }

    public Task<List<CastedAcceptancePollVoteEvent>> GetAllFor(Guid thingId)
    {
        return _dbContext.CastedAcceptancePollVoteEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId)
            .ToListAsync();
    }
}