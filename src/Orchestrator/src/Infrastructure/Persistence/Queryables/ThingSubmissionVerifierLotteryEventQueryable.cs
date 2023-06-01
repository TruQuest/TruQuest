using Microsoft.EntityFrameworkCore;

using Dapper;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;
using Application.Common.Models.QM;

namespace Infrastructure.Persistence.Queryables;

internal class ThingSubmissionVerifierLotteryEventQueryable : Queryable, IThingSubmissionVerifierLotteryEventQueryable
{
    private readonly EventDbContext _dbContext;

    public ThingSubmissionVerifierLotteryEventQueryable(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<JoinedThingSubmissionVerifierLotteryEvent>> FindJoinedEventsWithClosestNonces(
        Guid thingId, long latestBlockNumber, decimal nonce, int count
    )
    {
        return _dbContext.JoinedThingSubmissionVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.BlockNumber <= latestBlockNumber)
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.BlockNumber)
                    .ThenBy(e => e.TxnIndex)
            .Take(count)
            .ToListAsync();
    }

    public Task<List<JoinedThingSubmissionVerifierLotteryEvent>> GetJoinedEventsFor(
        Guid thingId, IEnumerable<string> userIds
    )
    {
        return _dbContext.JoinedThingSubmissionVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && userIds.Contains(e.UserId))
            .ToListAsync();
    }

    public async Task<IEnumerable<VerifierLotteryWinnerQm>> GetLotteryWinnerIndicesAccordingToPreJoinedEvents(
        Guid thingId, IEnumerable<string> winnerIds
    )
    {
        var dbConn = await _getOpenConnection();
        var winners = await dbConn.QueryAsync<VerifierLotteryWinnerQm>(
            @"
                WITH ""UserIdAndRowNumber"" AS (
                    SELECT
                        e.""UserId"",
                        ROW_NUMBER() OVER(ORDER BY e.""BlockNumber"", e.""TxnIndex"") AS ""RowNumber""
                    FROM truquest_events.""PreJoinedThingSubmissionVerifierLotteryEvents"" AS e
                    WHERE e.""ThingId"" = @ThingId
                )
                SELECT val.""UserId"", (val.""RowNumber"" - 2) AS ""Index""
                FROM ""UserIdAndRowNumber"" AS val
                WHERE val.""UserId"" = ANY(@WinnerIds)
                ORDER BY val.""RowNumber""
            ",
            param: new
            {
                ThingId = thingId,
                WinnerIds = winnerIds.ToArray()
            }
        );

        return winners;
    }
}