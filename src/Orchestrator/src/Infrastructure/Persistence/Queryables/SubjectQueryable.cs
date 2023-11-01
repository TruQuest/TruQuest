using Dapper;

using Application.Subject.Queries.GetSubject;
using Application.Common.Interfaces;
using Application.Common.Models.QM;
using Application.Subject.Common.Models.QM;
using Application.Subject.Queries.GetSubjects;

namespace Infrastructure.Persistence.Queryables;

internal class SubjectQueryable : Queryable, ISubjectQueryable
{
    public SubjectQueryable(
        AppDbContext appDbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(appDbContext, sharedTxnScope) { }

    public async Task<IEnumerable<SubjectPreviewQm>> GetAll()
    {
        var dbConn = await _getOpenConnection();
        var subjects = await dbConn.QueryWithMany<SubjectPreviewQm, TagQm>(
            @"
                SELECT
                    s.""Id"", s.""SubmittedAt"", s.""Name"", s.""Type"", s.""CroppedImageIpfsCid"",
                    s.""SettledThingsCount"", s.""AvgScore""::INTEGER,
                    t.*
                FROM
                    truquest.""Subjects"" AS s
                        INNER JOIN
                    truquest.""SubjectAttachedTags"" AS sat
                        ON s.""Id"" = sat.""SubjectId""
                        INNER JOIN
                    truquest.""Tags"" AS t
                        ON sat.""TagId"" = t.""Id""
                ORDER BY s.""SubmittedAt"" ASC
            ",
            joinedCollectionSelector: subject => subject.Tags
        );

        return subjects;
    }

    public async Task<SubjectQm?> GetById(Guid id)
    {
        var dbConn = await _getOpenConnection();
        var subject = await dbConn.SingleWithMany<SubjectQm, TagQm>(
            @"
                SELECT s.*, u.""WalletAddress"" AS ""SubmitterWalletAddress"", t.*
                FROM
                    truquest.""Subjects"" AS s
                        INNER JOIN
                    truquest.""AspNetUsers"" AS u
                        ON s.""SubmitterId"" = u.""Id""
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
