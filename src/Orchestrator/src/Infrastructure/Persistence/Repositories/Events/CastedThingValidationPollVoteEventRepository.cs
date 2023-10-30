using Microsoft.EntityFrameworkCore;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class CastedThingValidationPollVoteEventRepository : Repository, ICastedThingValidationPollVoteEventRepository
{
    private new readonly EventDbContext _dbContext;

    public CastedThingValidationPollVoteEventRepository(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(CastedThingValidationPollVoteEvent @event)
    {
        _dbContext.CastedThingValidationPollVoteEvents.Add(@event);
    }

    public Task<List<CastedThingValidationPollVoteEvent>> GetAllFor(Guid thingId)
    {
        return _dbContext.CastedThingValidationPollVoteEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId)
            .ToListAsync();
    }
}
