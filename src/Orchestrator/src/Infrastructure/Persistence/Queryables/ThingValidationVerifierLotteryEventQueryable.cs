using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Dapper;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;
using Application.Common.Models.QM;

namespace Infrastructure.Persistence.Queryables;

internal class ThingValidationVerifierLotteryEventQueryable : Queryable, IThingValidationVerifierLotteryEventQueryable
{
    private new readonly EventDbContext _dbContext;

    public ThingValidationVerifierLotteryEventQueryable(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<string> GetJoinedEventUserDataFor(Guid thingId, string walletAddress) =>
        _dbContext.JoinedThingValidationVerifierLotteryEvents
            .Where(e => e.ThingId == thingId && e.WalletAddress == walletAddress)
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
                FROM truquest_events.""ThingValidationVerifierLotteryInitializedEvents""
                WHERE ""ThingId"" = @ThingId;

                SELECT
                    ""TxnHash"", ""Payload"",
                    CASE
                        WHEN ""Type"" = @LotteryClosedWithSuccessType::truquest_events.thing_event_type
                            THEN (""Payload""->>'nonce')::BIGINT
                        ELSE NULL
                    END AS ""Nonce""
                FROM truquest_events.""ActionableThingRelatedEvents""
                WHERE ""ThingId"" = @ThingId AND ""Type"" IN (
                    @LotteryClosedWithSuccessType::truquest_events.thing_event_type,
                    @LotteryClosedInFailureType::truquest_events.thing_event_type
                );

                SELECT ""L1BlockNumber"", ""TxnHash"", ""UserId"", ""WalletAddress"", ""UserData"", ""Nonce""
                FROM truquest_events.""JoinedThingValidationVerifierLotteryEvents""
                WHERE ""ThingId"" = @ThingId
                ORDER BY ""BlockNumber"" DESC, ""TxnIndex"" DESC;
            ",
            param: new
            {
                ThingId = thingId,
                LotteryClosedWithSuccessType = ThingEventType.ValidationVerifierLotterySucceeded.GetString(),
                LotteryClosedInFailureType = ThingEventType.ValidationVerifierLotteryFailed.GetString()
            }
        );

        var orchestratorCommitment = await multiQuery.ReadSingleOrDefaultAsync<OrchestratorLotteryCommitmentQm?>();
        if (orchestratorCommitment == null) return (null, null, Enumerable.Empty<VerifierLotteryParticipantEntryQm>());

        var lotteryClosedEvent = await multiQuery.ReadSingleOrDefaultAsync<LotteryClosedEventQm?>();
        var participantEntries = await multiQuery.ReadAsync<VerifierLotteryParticipantEntryQm>();

        if (lotteryClosedEvent?.Nonce != null) // means successful lottery
        {
            foreach (var address in ((JsonElement)lotteryClosedEvent.Payload["winnerWalletAddresses"]).EnumerateArray())
            {
                participantEntries.Single(e => e.WalletAddress == address.GetString()!).MarkAsWinner();
            }

            participantEntries = participantEntries.OrderBy(e => e.SortKey);
        }

        return (orchestratorCommitment, lotteryClosedEvent, participantEntries);
    }
}
