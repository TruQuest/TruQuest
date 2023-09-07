using Microsoft.EntityFrameworkCore;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class ThingSubmissionVerifierLotteryEventQueryable : Queryable, IThingSubmissionVerifierLotteryEventQueryable
{
    private new readonly EventDbContext _dbContext;

    public ThingSubmissionVerifierLotteryEventQueryable(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public Task<JoinedThingSubmissionVerifierLotteryEvent> GetJoinedEventFor(
        Guid thingId, string userId
    ) => _dbContext.JoinedThingSubmissionVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.UserId == userId)
            .SingleAsync();
}
