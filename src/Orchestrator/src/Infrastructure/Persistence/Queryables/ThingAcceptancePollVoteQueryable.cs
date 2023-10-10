using Dapper;

using Application.Common.Models.QM;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class ThingAcceptancePollVoteQueryable : Queryable, IThingAcceptancePollVoteQueryable
{
    public ThingAcceptancePollVoteQueryable(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope) { }

    public async Task<(ThingAcceptancePollResultQm, IEnumerable<VoteQm>)> GetAllFor(Guid thingId, string? userId)
    {
        var dbConn = await _getOpenConnection();
        using var multiQuery = await dbConn.QueryMultipleAsync(
            @"
                SELECT ""State"", ""VoteAggIpfsCid""
                FROM truquest.""Things""
                WHERE ""Id"" = @ThingId;

                SELECT DISTINCT ON (""UserId"")
                    ""UserId"", ""CastedAtMs"", ""L1BlockNumber"", ""BlockNumber"",
                    ""Decision"", ""Reason"", ""IpfsCid"", ""TxnHash""
                FROM (
                    SELECT *
                    -- @@NOTE: We do this weird subquery because when using UNION can only order the whole result set,
                    -- but we need to order the parts separately for DISTINCT to do its job
                    FROM (
                        SELECT DISTINCT ON (""VoterId"")
                            ""VoterId"" AS ""UserId"",
                            1 AS ""OffChain"",
                            ""CastedAtMs"",
                            ""Decision"",
                            ""Reason"",
                            ""IpfsCid"",
                            NULL::BIGINT AS ""L1BlockNumber"",
                            NULL::BIGINT AS ""BlockNumber"",
                            NULL::TEXT AS ""TxnHash""
                        FROM truquest.""AcceptancePollVotes""
                        WHERE ""ThingId"" = @ThingId
                        ORDER BY ""VoterId"", ""CastedAtMs"" DESC
                    ) AS a

                    UNION ALL

                    SELECT *
                    FROM (
                        SELECT DISTINCT ON (""UserId"")
                            ""UserId"",
                            0 AS ""OffChain"",
                            NULL::BIGINT AS ""CastedAtMs"",
                            ""Decision"",
                            ""Reason"",
                            NULL::TEXT AS ""IpfsCid"",
                            ""L1BlockNumber"",
                            ""BlockNumber"",
                            ""TxnHash""
                        FROM truquest_events.""CastedAcceptancePollVoteEvents""
                        WHERE ""ThingId"" = @ThingId
                        ORDER BY ""UserId"", ""BlockNumber"" DESC, ""TxnIndex"" DESC
                    ) AS b
                ) AS c
                ORDER BY ""UserId"", ""OffChain"";
            ",
            param: new { ThingId = thingId }
        );

        var pollResult = multiQuery.ReadSingle<ThingAcceptancePollResultQm>();
        var votes = multiQuery.Read<VoteQm>();

        if (pollResult.VoteAggIpfsCid == null)
        {
            // If poll is not yet finalized do not show votes other than the user's own.
            foreach (var vote in votes)
            {
                if (vote.UserId != userId) vote.ClearSensitiveData();
            }
        }

        return (pollResult, votes);
    }
}
