using Microsoft.EntityFrameworkCore;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class ThingSubmissionVerifierLotteryEventQueryable : Queryable, IThingSubmissionVerifierLotteryEventQueryable
{
    private readonly EventDbContext _dbContext;

    public ThingSubmissionVerifierLotteryEventQueryable(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<JoinedThingSubmissionVerifierLotteryEvent>> GetJoinedEventsFor(
        Guid thingId, IEnumerable<string> userIds
    )
    {
        return _dbContext.JoinedThingSubmissionVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && userIds.Contains(e.UserId))
            .ToListAsync();
    }
}