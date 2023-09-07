using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class AcceptancePollVoteRepository : Repository, IAcceptancePollVoteRepository
{
    private new readonly AppDbContext _dbContext;

    public AcceptancePollVoteRepository(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(AcceptancePollVote vote)
    {
        _dbContext.AcceptancePollVotes.Add(vote);
    }

    public Task<List<AcceptancePollVote>> GetFor(Guid thingId) =>
        _dbContext.AcceptancePollVotes
            .AsNoTracking()
            .Where(v => v.ThingId == thingId)
            .ToListAsync();
}
