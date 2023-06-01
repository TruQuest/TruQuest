using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class AssessmentPollVoteRepository : Repository<AssessmentPollVote>, IAssessmentPollVoteRepository
{
    private readonly AppDbContext _dbContext;

    public AssessmentPollVoteRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(AssessmentPollVote vote)
    {
        _dbContext.AssessmentPollVotes.Add(vote);
    }

    public Task<List<AssessmentPollVote>> GetFor(Guid settlementProposalId) =>
        _dbContext.AssessmentPollVotes
            .AsNoTracking()
            .Where(v => v.SettlementProposalId == settlementProposalId)
            .ToListAsync();
}