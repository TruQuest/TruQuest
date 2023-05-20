using Microsoft.Extensions.Configuration;

using Dapper;

using Domain.Aggregates;
using Application.Subject.Queries.GetSubject;
using Application.Common.Interfaces;
using Application.Common.Models.QM;
using Application.Subject.Common.Models.QM;
using Application.Subject.Queries.GetSubjects;

namespace Infrastructure.Persistence.Queryables;

internal class SubjectQueryable : Queryable, ISubjectQueryable
{
    public SubjectQueryable(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<SubjectPreviewQm>> GetAll()
    {
        var dbConn = await _getOpenConnection();
        var subjects = await dbConn.QueryWithMany<SubjectPreviewQm, TagQm>(
            @"
                WITH ""SubjectAvgScore"" AS (
                    SELECT
                        s.""Id"",
                        COUNT(*)::INTEGER AS ""SettledThingsCount"",
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
                    GROUP BY s.""Id""
                )
                SELECT
                    s.""Id"", s.""SubmittedAt"", s.""Name"", s.""Type"", s.""CroppedImageIpfsCid"", s.""SubmitterId"",
                    sas.""SettledThingsCount"", sas.""AvgScore"",
                    t.*
                FROM
                    truquest.""Subjects"" AS s
                        LEFT JOIN
                    ""SubjectAvgScore"" AS sas
                        ON s.""Id"" = sas.""Id""
                        INNER JOIN
                    truquest.""SubjectAttachedTags"" AS sat
                        ON s.""Id"" = sat.""SubjectId""
                        INNER JOIN
                    truquest.""Tags"" AS t
                        ON sat.""TagId"" = t.""Id""
            ",
            joinedCollectionSelector: subject => subject.Tags,
            param: new { ThingState = (int)ThingState.Settled }
        );

        return subjects;
    }

    public async Task<SubjectQm?> GetById(Guid id)
    {
        var dbConn = await _getOpenConnection();
        var subject = await dbConn.SingleWithMany<SubjectQm, TagQm>(
            @"
                WITH ""SubjectAvgScore"" AS (
                    SELECT
                        s.""Id"",
                        COUNT(*)::INTEGER AS ""SettledThingsCount"",
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
                            ON (s.""Id"" = t.""SubjectId"" AND t.""State"" = 5) -- Settled
                            INNER JOIN
                        truquest.""SettlementProposals"" AS p
                            ON t.""AcceptedSettlementProposalId"" = p.""Id""
                    WHERE s.""Id"" = @SubjectId
                    GROUP BY s.""Id""
                )
                SELECT s.*, sas.""SettledThingsCount"", sas.""AvgScore"", t.*
                FROM
                    truquest.""Subjects"" AS s
                        LEFT JOIN
                    ""SubjectAvgScore"" AS sas
                        ON s.""Id"" = sas.""Id""
                        INNER JOIN
                    truquest.""SubjectAttachedTags"" AS sat
                        ON s.""Id"" = sat.""SubjectId""
                        INNER JOIN
                    truquest.""Tags"" AS t
                        ON sat.""TagId"" = t.""Id""
                WHERE s.""Id"" = @SubjectId
            ",
            joinedCollectionSelector: subject => subject.Tags,
            param: new { SubjectId = id }
        );

        if (subject != null)
        {
            using var multiQuery = await dbConn.QueryMultipleAsync(
                @"
                    SELECT ""Id"", ""State"", ""Title"", ""CroppedImageIpfsCid"", ""SettledAt"" AS ""DisplayedTimestamp""
                    FROM truquest.""Things""
                    WHERE ""SubjectId"" = @SubjectId AND ""SettledAt"" IS NOT NULL
                    ORDER BY ""SettledAt"" DESC
                    LIMIT 3;

                    SELECT ""Id"", ""State"", ""Title"", ""CroppedImageIpfsCid"", ""SubmittedAt"" AS ""DisplayedTimestamp""
                    FROM truquest.""Things""
                    WHERE ""SubjectId"" = @SubjectId AND ""SettledAt"" IS NULL AND ""SubmittedAt"" IS NOT NULL
                    ORDER BY ""SubmittedAt"" DESC
                    LIMIT 3;
                ",
                new { SubjectId = id }
            );

            subject.LatestSettledThings = (await multiQuery.ReadAsync<ThingPreviewQm>()).ToList();
            subject.LatestUnsettledThings = (await multiQuery.ReadAsync<ThingPreviewQm>()).ToList();
        }

        return subject;
    }
}