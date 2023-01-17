using Microsoft.Extensions.Configuration;

using Application.Thing.Queries.GetThing;
using Application.Common.Models.QM;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class ThingQueryable : Queryable, IThingQueryable
{
    public ThingQueryable(IConfiguration configuration) : base(configuration) { }

    public async Task<ThingQm?> GetById(Guid id)
    {
        var dbConn = await getOpenConnection();
        var thing = await dbConn.SingleWithMultipleMany<ThingQm, EvidenceQm, TagQm>(
            @"
                SELECT t.*, e.*, tag.*
                FROM
                    ""Things"" AS t
                        INNER JOIN
                    ""Evidence"" AS e
                        ON t.""Id"" = e.""ThingId""
                        LEFT JOIN
                    ""ThingAttachedTags"" AS tat
                        ON t.""Id"" = tat.""ThingId""
                        INNER JOIN
                    ""Tags"" AS tag
                        ON tat.""TagId"" = tag.""Id""
                WHERE t.""Id"" = @ThingId
            ",
            joined1CollectionSelector: thing => thing.Evidence,
            joined2CollectionSelector: thing => thing.Tags,
            param: new { ThingId = id }
        );

        return thing;
    }
}