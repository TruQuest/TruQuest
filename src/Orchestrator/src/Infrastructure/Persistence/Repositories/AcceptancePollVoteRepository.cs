using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class AcceptancePollVoteRepository : Repository<AcceptancePollVote>, IAcceptancePollVoteRepository
{
    private readonly AppDbContext _dbContext;

    public AcceptancePollVoteRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
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