using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;

namespace Infrastructure.Persistence.Repositories;

internal class SettlementProposalAssessmentPollVoteRepository : Repository, ISettlementProposalAssessmentPollVoteRepository
{
    private new readonly AppDbContext _dbContext;

    public SettlementProposalAssessmentPollVoteRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(SettlementProposalAssessmentPollVote vote)
    {
        _dbContext.SettlementProposalAssessmentPollVotes.Add(vote);
    }

    public Task<List<SettlementProposalAssessmentPollVote>> GetFor(Guid proposalId) =>
        _dbContext.SettlementProposalAssessmentPollVotes
            .AsNoTracking()
            .Where(v => v.SettlementProposalId == proposalId)
            .ToListAsync();
}
