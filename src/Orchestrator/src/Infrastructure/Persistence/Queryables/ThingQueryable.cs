using Microsoft.Extensions.Configuration;

using Dapper;

using Domain.Aggregates;
using Application.Thing.Queries.GetThing;
using Application.Common.Models.QM;
using Application.Common.Interfaces;
using Application.Subject.Common.Models.QM;

namespace Infrastructure.Persistence.Queryables;

internal class ThingQueryable : Queryable, IThingQueryable
{
    public ThingQueryable(IConfiguration configuration) : base(configuration) { }

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
                    t.""State"" != @ConsensusNotReachedState AND
                    (t.""State"" > @ThingState OR t.""SubmitterId"" = @UserId);
                -- @@TODO: Check that it works with UserId == null as intended
            ",
            joinedCollectionSelector: thing => thing.Tags,
            param: new
            {
                SubjectId = subjectId,
                UserId = userId,
                ConsensusNotReachedState = (int)ThingState.ConsensusNotReached,
                ThingState = (int)ThingState.Draft
            }
        );

        return things;
    }

    public async Task<ThingQm?> GetById(Guid id, string? userId)
    {
        var dbConn = await _getOpenConnection();
        using var multiQuery = await dbConn.QueryMultipleAsync(
            @"
                SELECT
                    t.*,
                    s.""Name"" AS ""SubjectName"", s.""CroppedImageIpfsCid"" AS ""SubjectCroppedImageIpfsCid"",
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

            var result = await dbConn.QuerySingleAsync(
                @"
                    SELECT
                        AVG(
                            CASE
                                WHEN p.""Verdict"" = 0 THEN 100
                                WHEN p.""Verdict"" = 1 THEN 75
                                WHEN p.""Verdict"" = 2 THEN 40
                                WHEN p.""Verdict"" = 3 THEN 0
                                WHEN p.""Verdict"" = 4 THEN -40
                                WHEN p.""Verdict"" = 5 THEN -100
                            END
                        )::INTEGER AS ""AvgScore""
                    FROM
                        truquest.""Subjects"" AS s
                            INNER JOIN
                        truquest.""Things"" AS t
                            ON (s.""Id"" = t.""SubjectId"" AND t.""State"" = @ThingState)
                            INNER JOIN
                        truquest.""SettlementProposals"" AS p
                            ON t.""AcceptedSettlementProposalId"" = p.""Id""
                    WHERE s.""Id"" = @SubjectId
                ",
                param: new
                {
                    SubjectId = thing.SubjectId,
                    ThingState = (int)ThingState.Settled
                }
            );
            thing.SubjectAvgScore = result.AvgScore;
        }

        return thing;
    }

    public async Task<IEnumerable<VerifierLotteryParticipantEntryQm>> GetVerifierLotteryParticipants(Guid thingId)
    {
        var dbConn = await _getOpenConnection();
        var entries = await dbConn.QueryAsync<VerifierLotteryParticipantEntryQm>(
            @"
                SELECT je.""BlockNumber"" AS ""JoinedBlockNumber"", ""UserId"", pje.""DataHash"", je.""Nonce""
                FROM
                    truquest_events.""PreJoinedThingSubmissionVerifierLotteryEvents"" AS pje
                        LEFT JOIN
                    truquest_events.""JoinedThingSubmissionVerifierLotteryEvents"" AS je
                        USING (""ThingId"", ""UserId"")
                WHERE ""ThingId"" = @ThingId
                ORDER BY je.""BlockNumber"" DESC NULLS LAST, je.""TxnIndex"" DESC
            ",
            param: new { ThingId = thingId }
        );

        return entries;
    }

    public async Task<IEnumerable<VerifierQm>> GetVerifiers(Guid thingId)
    {
        var dbConn = await _getOpenConnection();
        var verifiers = await dbConn.QueryAsync<VerifierQm>(
            @"
                SELECT v.""VerifierId"", u.""UserName""
                FROM
                    truquest.""Things"" AS t
                        INNER JOIN
                    truquest.""ThingVerifiers"" AS v
                        ON t.""Id"" = v.""ThingId""
                        INNER JOIN
                    truquest.""AspNetUsers"" AS u
                        ON v.""VerifierId"" = u.""Id""
                WHERE t.""Id"" = @ThingId
            ",
            param: new { ThingId = thingId }
        );

        return verifiers;
    }
}