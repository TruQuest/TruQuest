using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Dapper;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;
using Application.Common.Models.QM;

namespace Infrastructure.Persistence.Queryables;

internal class ThingSubmissionVerifierLotteryEventQueryable : Queryable, IThingSubmissionVerifierLotteryEventQueryable
{
    private new readonly EventDbContext _dbContext;

    public ThingSubmissionVerifierLotteryEventQueryable(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public Task<string> GetJoinedEventUserDataFor(Guid thingId, string userId) =>
        _dbContext.JoinedThingSubmissionVerifierLotteryEvents
            .Where(e => e.ThingId == thingId && e.UserId == userId)
            .Select(e => e.UserData)
            .SingleAsync();

    public async Task<(
        OrchestratorLotteryCommitmentQm?,
        LotteryClosedEventQm?,
        IEnumerable<VerifierLotteryParticipantEntryQm>
    )> GetOrchestratorCommitmentAndParticipants(Guid thingId)
    {
        var dbConn = await _getOpenConnection();
        using var multiQuery = await dbConn.QueryMultipleAsync(
            @"
                SELECT ""L1BlockNumber"", ""TxnHash"", ""DataHash"", ""UserXorDataHash""
                FROM truquest_events.""ThingSubmissionVerifierLotteryInitializedEvents""
                WHERE ""ThingId"" = @ThingId;

                SELECT
                    ""TxnHash"", ""Payload"",
                    CASE
                        WHEN ""Type"" = @LotteryClosedWithSuccessType
                            THEN (""Payload""->>'nonce')::BIGINT
                        ELSE NULL
                    END AS ""Nonce""
                FROM truquest_events.""ActionableThingRelatedEvents""
                WHERE ""ThingId"" = @ThingId AND ""Type"" IN (@LotteryClosedWithSuccessType, @LotteryClosedInFailureType);

                SELECT ""L1BlockNumber"", ""TxnHash"", ""UserId"", ""UserData"", ""Nonce""
                FROM truquest_events.""JoinedThingSubmissionVerifierLotteryEvents""
                WHERE ""ThingId"" = @ThingId
                ORDER BY ""BlockNumber"" DESC, ""TxnIndex"" DESC;
            ",
            param: new
            {
                ThingId = thingId,
                LotteryClosedWithSuccessType = (int)ThingEventType.SubmissionVerifierLotteryClosedWithSuccess,
                LotteryClosedInFailureType = (int)ThingEventType.SubmissionVerifierLotteryClosedInFailure
            }
        );

        var orchestratorCommitment = await multiQuery.ReadSingleOrDefaultAsync<OrchestratorLotteryCommitmentQm?>();
        if (orchestratorCommitment == null) return (null, null, Enumerable.Empty<VerifierLotteryParticipantEntryQm>());

        var lotteryClosedEvent = await multiQuery.ReadSingleOrDefaultAsync<LotteryClosedEventQm?>();
        var participantEntries = await multiQuery.ReadAsync<VerifierLotteryParticipantEntryQm>();

        if (lotteryClosedEvent?.Nonce != null) // means successful lottery
        {
            foreach (var winnerId in ((JsonElement)lotteryClosedEvent.Payload["winnerIds"]).EnumerateArray())
            {
                participantEntries.Single(e => e.UserId == winnerId.GetString()!).MarkAsWinner();
            }

            participantEntries = participantEntries.OrderBy(e => e.SortKey);
        }

        return (orchestratorCommitment, lotteryClosedEvent, participantEntries);
    }
}
