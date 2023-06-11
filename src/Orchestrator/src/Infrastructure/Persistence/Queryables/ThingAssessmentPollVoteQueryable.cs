using Microsoft.EntityFrameworkCore;

using Dapper;

using Application.Common.Models.QM;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class ThingAssessmentPollVoteQueryable : Queryable, IThingAssessmentPollVoteQueryable
{
    private readonly AppDbContext _dbContext;

    public ThingAssessmentPollVoteQueryable(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<VoteQm>> GetAllFor(Guid proposalId)
    {
        var offChainVotes = await _dbContext.AssessmentPollVotes
            .AsNoTracking()
            .Where(v => v.SettlementProposalId == proposalId)
            .OrderBy(v => v.CastedAtMs)
            .Select(v => new VoteQm
            {
                UserId = v.VoterId,
                CastedAtMs = v.CastedAtMs
            })
            .ToListAsync();

        var dbConn = await _getOpenConnection();
        var onChainVotes = await dbConn.QueryAsync<VoteQm>(
            @"
                SELECT e.""UserId"", e.""L1BlockNumber"" AS ""BlockNumber""
                FROM truquest_events.""CastedAssessmentPollVoteEvents"" AS e
                WHERE e.""SettlementProposalId"" = @ProposalId
                ORDER BY e.""BlockNumber"", e.""TxnIndex""
            ",
            param: new { ProposalId = proposalId }
        );

        return offChainVotes.Concat(onChainVotes);
    }
}