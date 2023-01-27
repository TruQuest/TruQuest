using Microsoft.EntityFrameworkCore;

using Dapper;

using Domain.Aggregates.Events;
using Application.Common.Models.QM;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class ThingAssessmentPollEventQueryable : Queryable, IThingAssessmentPollEventQueryable
{
    private readonly EventDbContext _dbContext;

    public ThingAssessmentPollEventQueryable(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ThingAssessmentVerifierLotterySpotClaimedEvent>> FindAllSpotClaimedEventsFor(
        Guid thingId, Guid settlementProposalId
    )
    {
        return _dbContext.ThingAssessmentVerifierLotterySpotClaimedEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.SettlementProposalId == settlementProposalId)
            .OrderBy(e => e.BlockNumber)
                .ThenBy(e => e.TxnIndex)
            .ToListAsync();
    }

    public Task<List<JoinedThingAssessmentVerifierLotteryEvent>> FindJoinedEventsWithClosestNonces(
        Guid thingId, Guid settlementProposalId, long latestBlockNumber, decimal nonce, int count
    )
    {
        return _dbContext.JoinedThingAssessmentVerifierLotteryEvents
            .AsNoTracking()
            .Where(e =>
                e.ThingId == thingId &&
                e.SettlementProposalId == settlementProposalId &&
                e.BlockNumber <= latestBlockNumber
            )
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.BlockNumber)
                    .ThenBy(e => e.TxnIndex)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<VerifierLotteryWinnerQm>> GetLotteryWinnerIndicesAccordingToPreJoinedEvents(
        Guid thingId, Guid settlementProposalId, IEnumerable<string> winnerIds
    )
    {
        var dbConn = await _getOpenConnection();
        var winners = await dbConn.QueryAsync<VerifierLotteryWinnerQm>(
            @"
                WITH ""UserIdAndRowNumber"" AS (
                    SELECT
                        e.""UserId"",
                        ROW_NUMBER() OVER(ORDER BY e.""BlockNumber"", e.""TxnIndex"") AS ""RowNumber""
                    FROM truquest_events.""PreJoinedThingAssessmentVerifierLotteryEvents"" AS e
                    WHERE e.""ThingId"" = @ThingId AND e.""SettlementProposalId"" = @ProposalId
                )
                SELECT val.""UserId"", (val.""RowNumber"" - 2) AS ""Index""
                FROM ""UserIdAndRowNumber"" AS val
                WHERE val.""UserId"" = ANY(@WinnerIds)
                ORDER BY val.""RowNumber""
            ",
            param: new
            {
                ThingId = thingId,
                ProposalId = settlementProposalId,
                WinnerIds = winnerIds.ToArray()
            }
        );

        return winners;
    }
}