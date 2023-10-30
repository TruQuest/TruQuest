using System.Text.Json;

using Dapper;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;
using Application.Common.Models.QM;

namespace Infrastructure.Persistence.Queryables;

internal class SettlementProposalAssessmentVerifierLotteryEventQueryable :
    Queryable,
    ISettlementProposalAssessmentVerifierLotteryEventQueryable
{
    public SettlementProposalAssessmentVerifierLotteryEventQueryable(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope) { }

    public async Task<(
        OrchestratorLotteryCommitmentQm?,
        LotteryClosedEventQm?,
        IEnumerable<VerifierLotteryParticipantEntryQm> Participants,
        IEnumerable<VerifierLotteryParticipantEntryQm> Claimants
    )> GetOrchestratorCommitmentAndParticipants(Guid thingId, Guid proposalId)
    {
        var dbConn = await _getOpenConnection();
        using var multiQuery = await dbConn.QueryMultipleAsync(
            @"
                SELECT ""L1BlockNumber"", ""TxnHash"", ""DataHash"", ""UserXorDataHash""
                FROM truquest_events.""SettlementProposalAssessmentVerifierLotteryInitializedEvents""
                WHERE ""SettlementProposalId"" = @ProposalId;

                SELECT
                    ""TxnHash"", ""Payload"",
                    CASE
                        WHEN ""Type"" = @LotteryClosedWithSuccessType
                            THEN (""Payload""->>'nonce')::BIGINT
                        ELSE NULL
                    END AS ""Nonce""
                FROM truquest_events.""ActionableThingRelatedEvents""
                WHERE
                    ""ThingId"" = @ThingId AND
                    ""Type"" IN (@LotteryClosedWithSuccessType, @LotteryClosedInFailureType) AND
                    ""Payload""->>'settlementProposalId' = @ProposalId::TEXT;

                SELECT ""L1BlockNumber"", ""TxnHash"", ""UserId"", ""WalletAddress"", ""UserData"", ""Nonce""
                FROM truquest_events.""JoinedSettlementProposalAssessmentVerifierLotteryEvents""
                WHERE ""SettlementProposalId"" = @ProposalId
                ORDER BY ""BlockNumber"" DESC, ""TxnIndex"" DESC;

                SELECT ""L1BlockNumber"", ""TxnHash"", ""UserId"", ""WalletAddress"", ""UserData"", ""Nonce""
                FROM truquest_events.""ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents""
                WHERE ""SettlementProposalId"" = @ProposalId
                ORDER BY ""BlockNumber"" DESC, ""TxnIndex"" DESC;
            ",
            param: new
            {
                ThingId = thingId,
                ProposalId = proposalId,
                LotteryClosedWithSuccessType = (int)ThingEventType.SettlementProposalAssessmentVerifierLotteryClosedWithSuccess,
                LotteryClosedInFailureType = (int)ThingEventType.SettlementProposalAssessmentVerifierLotteryClosedInFailure
            }
        );

        var orchestratorCommitment = await multiQuery.ReadSingleOrDefaultAsync<OrchestratorLotteryCommitmentQm?>();
        if (orchestratorCommitment == null) return (
            null,
            null,
            Enumerable.Empty<VerifierLotteryParticipantEntryQm>(),
            Enumerable.Empty<VerifierLotteryParticipantEntryQm>()
        );

        var lotteryClosedEvent = await multiQuery.ReadSingleOrDefaultAsync<LotteryClosedEventQm?>();
        var participantEntries = await multiQuery.ReadAsync<VerifierLotteryParticipantEntryQm>();
        var claimantEntries = await multiQuery.ReadAsync<VerifierLotteryParticipantEntryQm>();

        if (lotteryClosedEvent?.Nonce != null) // means successful lottery
        {
            foreach (var address in ((JsonElement)lotteryClosedEvent.Payload["winnerWalletAddresses"]).EnumerateArray())
            {
                participantEntries.Single(e => e.WalletAddress == address.GetString()!).MarkAsWinner();
            }
            foreach (var address in ((JsonElement)lotteryClosedEvent.Payload["claimantWalletAddresses"]).EnumerateArray())
            {
                claimantEntries.Single(e => e.WalletAddress == address.GetString()!).MarkAsWinner();
            }

            participantEntries = participantEntries.OrderBy(e => e.SortKey);
            claimantEntries = claimantEntries.OrderBy(e => e.SortKey);
        }

        // @@NOTE: It's possible that the lottery is already closed (and therefore nonces are set)
        // but the corresponding ActionableThingRelatedEvent is not yet created, in which case
        // all participants and claimants will have their nonces displayed but there won't be
        // any 'winner' marks since winners are only marked when there is a non-null 'lotteryClosedEvent'.
        // Can just hide nonces in UI.

        return (orchestratorCommitment, lotteryClosedEvent, participantEntries, claimantEntries);
    }
}
