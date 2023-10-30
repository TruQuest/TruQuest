using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class ThingValidationPollVoteRepository : Repository, IThingValidationPollVoteRepository
{
    private new readonly AppDbContext _dbContext;

    public ThingValidationPollVoteRepository(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(ThingValidationPollVote vote)
    {
        _dbContext.ThingValidationPollVotes.Add(vote);
    }

    public Task<List<ThingValidationPollVote>> GetFor(Guid thingId) =>
        _dbContext.ThingValidationPollVotes
            .AsNoTracking()
            .Where(v => v.ThingId == thingId)
            .ToListAsync();
}
