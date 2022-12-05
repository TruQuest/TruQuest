using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class VoteRepository : Repository<Vote>, IVoteRepository
{
    private readonly AppDbContext _dbContext;

    public VoteRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(Vote vote)
    {
        _dbContext.Votes.Add(vote);
    }
}