using Dapper;

using Domain.Aggregates;
using Application.Thing.Queries.GetThing;
using Application.Common.Models.QM;
using Application.Common.Interfaces;
using Application.Subject.Common.Models.QM;

namespace Infrastructure.Persistence.Queryables;

internal class ThingQueryable : Queryable, IThingQueryable
{
    public ThingQueryable(AppDbContext dbContext) : base(dbContext) { }

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
                    t.""State"" != @LotteryFailedState::truquest.thing_state AND
                    t.""State"" != @ConsensusNotReachedState::truquest.thing_state AND
                    (t.""State"" != @DraftState::truquest.thing_state OR t.""SubmitterId"" = @UserId);
            ",
            joinedCollectionSelector: thing => thing.Tags,
            param: new
            {
                SubjectId = subjectId,
                UserId = userId,
                LotteryFailedState = ThingState.VerifierLotteryFailed.GetString(),
                ConsensusNotReachedState = ThingState.ConsensusNotReached.GetString(),
                DraftState = ThingState.Draft.GetString()
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
                SELECT -- @@NOTE: Conversion from Postgres enum to ThingStateQm enum just magically works in Dapper.
                    t.*, u.""WalletAddress"" AS ""SubmitterWalletAddress"",
                    s.""Name"" AS ""SubjectName"", s.""CroppedImageIpfsCid"" AS ""SubjectCroppedImageIpfsCid"",
                    s.""AvgScore""::INTEGER AS ""SubjectAvgScore"",
                    e.*, tag.*
                FROM
                    truquest.""Things"" AS t
                        INNER JOIN
                    truquest.""AspNetUsers"" AS u
                        ON t.""SubmitterId"" = u.""Id""
                        INNER JOIN
                    truquest.""Subjects"" AS s
                        ON t.""SubjectId"" = s.""Id""
                        INNER JOIN
                    truquest.""ThingEvidence"" AS e
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
                    (@UserId, @ItemType::truquest.watched_item_type, @ItemId, @ItemUpdateCategory);
            ",
            param: new
            {
                UserId = userId,
                ItemType = WatchedItemType.Thing.GetString(),
                ItemId = id,
                ItemUpdateCategory = (int)ThingUpdateCategory.General
            }
        );

        var thing = multiQuery.SingleWithMultipleMany<ThingQm, ThingEvidenceQm, TagQm>(
            joined1CollectionSelector: thing => thing.Evidence,
            joined2CollectionSelector: thing => thing.Tags
        );
        if (thing != null)
        {
            thing.Watched = multiQuery.ReadSingleOrDefault<int?>() != null;
        }

        return thing;
    }
}
