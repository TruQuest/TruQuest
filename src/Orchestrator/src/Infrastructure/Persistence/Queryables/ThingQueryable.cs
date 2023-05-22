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
                WHERE t.""SubjectId"" = @SubjectId AND (t.""State"" > 0 OR t.""SubmitterId"" = @UserId);
                -- @@TODO: Check that it works with UserId == null as intended
            ",
            joinedCollectionSelector: thing => thing.Tags,
            param: new
            {
                SubjectId = subjectId,
                UserId = userId
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
                WHERE t.""Id"" = @ThingId;

                SELECT COUNT(*)
                FROM truquest.""WatchList""
                WHERE ""UserId"" = @UserId AND ""ItemType"" = @ItemType AND ""ItemId"" = @ThingId;
            ",
            param: new
            {
                ThingId = id,
                UserId = userId,
                ItemType = (int)WatchedItemType.Thing
            }
        );

        var thing = multiQuery.SingleWithMultipleMany<ThingQm, EvidenceQm, TagQm>(
            joined1CollectionSelector: thing => thing.Evidence,
            joined2CollectionSelector: thing => thing.Tags
        );
        if (thing != null)
        {
            thing.Watched = multiQuery.ReadSingleOrDefault<long>() != 0;
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