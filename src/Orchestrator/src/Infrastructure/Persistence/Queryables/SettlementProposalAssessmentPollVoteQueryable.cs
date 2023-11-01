using Dapper;

using Application.Common.Models.QM;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class SettlementProposalAssessmentPollVoteQueryable : Queryable, ISettlementProposalAssessmentPollVoteQueryable
{
    public SettlementProposalAssessmentPollVoteQueryable(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope) { }

    public async Task<(SettlementProposalAssessmentPollResultQm, IEnumerable<VoteQm>)> GetAllFor(Guid proposalId, string? userId)
    {
        var dbConn = await _getOpenConnection();
        using var multiQuery = await dbConn.QueryMultipleAsync(
            @"
                SELECT ""State"", ""VoteAggIpfsCid""
                FROM truquest.""SettlementProposals""
                WHERE ""Id"" = @ProposalId;

                SELECT DISTINCT ON (""UserId"")
                    ""UserId"", ""WalletAddress"", ""CastedAtMs"", ""L1BlockNumber"", ""BlockNumber"",
                    ""Decision"", ""Reason"", ""IpfsCid"", ""TxnHash""
                FROM (
                    SELECT *
                    FROM (
                        SELECT DISTINCT ON (""VoterId"")
                            ""VoterId"" AS ""UserId"",
                            ""VoterWalletAddress"" AS ""WalletAddress"",
                            1 AS ""OffChain"",
                            ""CastedAtMs"",
                            ""Decision"",
                            ""Reason"",
                            ""IpfsCid"",
                            NULL::BIGINT AS ""L1BlockNumber"",
                            NULL::BIGINT AS ""BlockNumber"",
                            NULL::TEXT AS ""TxnHash""
                        FROM truquest.""SettlementProposalAssessmentPollVotes""
                        WHERE ""SettlementProposalId"" = @ProposalId
                        ORDER BY ""VoterId"", ""CastedAtMs"" DESC
                    ) AS a

                    UNION ALL

                    SELECT *
                    FROM (
                        SELECT DISTINCT ON (""UserId"")
                            ""UserId"",
                            ""WalletAddress"",
                            0 AS ""OffChain"",
                            NULL::BIGINT AS ""CastedAtMs"",
                            ""Decision"",
                            ""Reason"",
                            NULL::TEXT AS ""IpfsCid"",
                            ""L1BlockNumber"",
                            ""BlockNumber"",
                            ""TxnHash""
                        FROM truquest_events.""CastedSettlementProposalAssessmentPollVoteEvents""
                        WHERE ""SettlementProposalId"" = @ProposalId
                        ORDER BY ""UserId"", ""BlockNumber"" DESC, ""TxnIndex"" DESC
                    ) AS b
                ) AS c
                ORDER BY ""UserId"", ""OffChain"";
            ",
            param: new { ProposalId = proposalId }
        );

        var pollResult = multiQuery.ReadSingle<SettlementProposalAssessmentPollResultQm>();
        var votes = multiQuery.Read<VoteQm>();

        if (pollResult.VoteAggIpfsCid == null)
        {
            // If poll is not yet finalized do not show votes other than the user's own one.
            foreach (var vote in votes)
            {
                if (vote.UserId != userId) vote.ClearSensitiveData();
            }
        }

        return (pollResult, votes);
    }
}
