using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class AssessmentPollVoteRepository : Repository, IAssessmentPollVoteRepository
{
    private new readonly AppDbContext _dbContext;

    public AssessmentPollVoteRepository(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
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
