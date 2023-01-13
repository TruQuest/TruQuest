using Microsoft.Extensions.Configuration;

using Application.Subject.Queries.GetSubject;
using Application.Common.Interfaces;
using Application.Common.Models.QM;

namespace Infrastructure.Persistence.Queryables;

internal class SubjectQueryable : Queryable, ISubjectQueryable
{
    public SubjectQueryable(IConfiguration configuration) : base(configuration) { }

    public async Task<SubjectQm?> GetById(Guid id)
    {
        var dbConn = await getOpenConnection();
        var subject = await dbConn.SingleWithMany<SubjectQm, TagQm, Guid>(
            @"
                SELECT s.*, t.*
                FROM
                    ""Subjects"" s
                        LEFT JOIN
                    ""SubjectAttachedTags"" st
                        ON s.""Id"" = st.""SubjectId""
                        LEFT JOIN
                    ""Tags"" t
                        ON st.""TagId"" = t.""Id""
                WHERE s.""Id"" = @SubjectId
            ",
            rootKeySelector: subject => subject.Id,
            joinedCollectionSelector: subject => subject.Tags,
            param: new
            {
                SubjectId = id
            }
        );

        return subject;
    }
}