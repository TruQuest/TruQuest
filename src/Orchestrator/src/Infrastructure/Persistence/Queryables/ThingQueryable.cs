using Dapper;

using Domain.Aggregates;
using Application.Thing.Queries.GetThing;
using Application.Common.Models.QM;
using Application.Common.Interfaces;
using Application.Subject.Common.Models.QM;

namespace Infrastructure.Persistence.Queryables;

internal class ThingQueryable : Queryable, IThingQueryable
{
    public ThingQueryable(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope) { }

    public async Task<IEnumerable<ThingPreviewQm>> GetForSubject(Guid subjectId, string? userId)
    {
        var dbConn = await _getOpenConnection();
        var things = await dbConn.QueryWithMany<ThingPreviewQm, TagQm>(
            @"
                SELECT
                    t.""Id"", t.""State"", t.""Title"", t.""CroppedImageIpfsCid"",
                    COALESCE(t.""SettledAt"", t.""SubmittedAt"") AS ""DisplayedTimestamp"",
                    p.""Verdict"",
                    tag.*
                FROM
                    truquest.""Things"" AS t
                        LEFT JOIN
                    truquest.""SettlementProposals"" AS p
                        ON t.""AcceptedSettlementProposalId"" = p.""Id""
                        INNER JOIN
                    truquest.""ThingAttachedTags"" AS tat
                        ON (t.""Id"" = tat.""ThingId"")
                        INNER JOIN
                    truquest.""Tags"" AS tag
                        ON (tat.""TagId"" = tag.""Id"")
                WHERE
                    t.""SubjectId"" = @SubjectId AND
                    t.""State"" != @LotteryFailedState AND
                    t.""State"" != @ConsensusNotReachedState AND
                    (t.""State"" > @DraftState OR t.""SubmitterId"" = @UserId);
            ",
            joinedCollectionSelector: thing => thing.Tags,
            param: new
            {
                SubjectId = subjectId,
                UserId = userId,
                LotteryFailedState = (int)ThingState.VerifierLotteryFailed,
                ConsensusNotReachedState = (int)ThingState.ConsensusNotReached,
                DraftState = (int)ThingState.Draft
            }
        );

        return things;
    }

    public async Task<ThingQm?> GetById(Guid id, string? userId)
    {
        // @@??: Shouldn't we handle the case when userId == null separately ?
        var dbConn = await _getOpenConnection();
        using var multiQuery = await dbConn.QueryMultipleAsync(
            @"
                SELECT
                    t.*,
                    s.""Name"" AS ""SubjectName"", s.""CroppedImageIpfsCid"" AS ""SubjectCroppedImageIpfsCid"",
                    s.""AvgScore""::INTEGER AS ""SubjectAvgScore"",
                    e.*, tag.*
                FROM
                    truquest.""Things"" AS t
                        INNER JOIN
                    truquest.""Subjects"" AS s
                        ON t.""SubjectId"" = s.""Id""
                        INNER JOIN
                    truquest.""Evidence"" AS e
                        ON t.""Id"" = e.""ThingId""
                        INNER JOIN
                    truquest.""ThingAttachedTags"" AS tat
                        ON t.""Id"" = tat.""ThingId""
                        INNER JOIN
                    truquest.""Tags"" AS tag
                        ON tat.""TagId"" = tag.""Id""
                WHERE t.""Id"" = @ItemId;

                SELECT 1
                FROM truquest.""WatchList""
                WHERE
                    (""UserId"", ""ItemType"", ""ItemId"", ""ItemUpdateCategory"") =
                    (@UserId, @ItemType, @ItemId, @ItemUpdateCategory);
            ",
            param: new
            {
                UserId = userId,
                ItemType = (int)WatchedItemType.Thing,
                ItemId = id,
                ItemUpdateCategory = (int)ThingUpdateCategory.General
            }
        );

        var thing = multiQuery.SingleWithMultipleMany<ThingQm, EvidenceQm, TagQm>(
            joined1CollectionSelector: thing => thing.Evidence,
            joined2CollectionSelector: thing => thing.Tags
        );
        if (thing != null)
        {
            thing.Watched = multiQuery.ReadSingleOrDefault<int?>() != null;
        }

        return thing;
    }

    public async Task<ThingState> GetStateFor(Guid thingId)
    {
        var dbConn = await _getOpenConnection();
        return await dbConn.QuerySingleAsync<ThingState>(
            @"
                SELECT ""State""
                FROM truquest.""Things""
                WHERE ""ThingId"" = @ThingId;
            ",
            param: new { ThingId = thingId }
        );
    }

    public async Task<IEnumerable<VerifierLotteryParticipantEntryQm>> GetVerifierLotteryParticipants(Guid thingId)
    {
        var dbConn = await _getOpenConnection();
        var entries = await dbConn.QueryAsync<VerifierLotteryParticipantEntryQm>(
            @"
                SELECT ""L1BlockNumber"", ""BlockNumber"", ""TxnHash"", ""UserId"", ""UserData"", ""Nonce""
                FROM truquest_events.""JoinedThingSubmissionVerifierLotteryEvents""
                WHERE ""ThingId"" = @ThingId
                ORDER BY ""BlockNumber"" DESC, ""TxnIndex"" DESC
            ",
            param: new { ThingId = thingId }
        );

        return entries;
    }

    public async Task<IEnumerable<string>> GetVerifiers(Guid thingId)
    {
        var dbConn = await _getOpenConnection();
        var verifiers = await dbConn.QueryAsync<string>(
            @"
                SELECT v.""VerifierId""
                FROM
                    truquest.""Things"" AS t
                        INNER JOIN
                    truquest.""ThingVerifiers"" AS v
                        ON t.""Id"" = v.""ThingId""
                WHERE t.""Id"" = @ThingId
            ",
            param: new { ThingId = thingId }
        );

        return verifiers;
    }
}
