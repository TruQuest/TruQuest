using Microsoft.EntityFrameworkCore;

using Dapper;

using Application.Common.Models.QM;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class ThingAcceptancePollVoteQueryable : Queryable, IThingAcceptancePollVoteQueryable
{
    private readonly AppDbContext _dbContext;

    public ThingAcceptancePollVoteQueryable(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<VoteQm>> GetAllFor(Guid thingId)
    {
        var offChainVotes = await _dbContext.AcceptancePollVotes
            .AsNoTracking()
            .Where(v => v.ThingId == thingId)
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
                FROM truquest_events.""CastedAcceptancePollVoteEvents"" AS e
                WHERE e.""ThingId"" = @ThingId
                ORDER BY e.""BlockNumber"", e.""TxnIndex""
            ",
            param: new
            {
                ThingId = thingId
            }
        );

        return offChainVotes.Concat(onChainVotes);
    }
}