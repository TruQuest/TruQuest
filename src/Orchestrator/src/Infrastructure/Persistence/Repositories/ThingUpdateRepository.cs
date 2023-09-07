using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class ThingUpdateRepository : Repository, IThingUpdateRepository
{
    public ThingUpdateRepository(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope) { }

    public async Task AddOrUpdate(params ThingUpdate[] updateEvents)
    {
        var thingIdsParam = new NpgsqlParameter<Guid[]>("ThingIds", NpgsqlDbType.Uuid | NpgsqlDbType.Array)
        {
            TypedValue = updateEvents.Select(e => e.ThingId).ToArray()
        };
        var categoriesParam = new NpgsqlParameter<int[]>("Categories", NpgsqlDbType.Integer | NpgsqlDbType.Array)
        {
            TypedValue = updateEvents.Select(e => (int)e.Category).ToArray()
        };
        var tsParam = new NpgsqlParameter<long[]>("UpdateTimestamps", NpgsqlDbType.Bigint | NpgsqlDbType.Array)
        {
            TypedValue = updateEvents.Select(e => e.UpdateTimestamp).ToArray()
        };
        var titlesParam = new NpgsqlParameter<string[]>("Titles", NpgsqlDbType.Text | NpgsqlDbType.Array)
        {
            TypedValue = updateEvents.Select(e => e.Title).ToArray()
        };
        var detailsParam = new NpgsqlParameter<string?[]>("Details", NpgsqlDbType.Text | NpgsqlDbType.Array)
        {
            TypedValue = updateEvents.Select(e => e.Details).ToArray()
        };

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                INSERT INTO truquest.""ThingUpdates"" (
                    ""ThingId"", ""Category"", ""UpdateTimestamp"", ""Title"", ""Details""
                )
                SELECT *
                FROM UNNEST(@ThingIds, @Categories, @UpdateTimestamps, @Titles, @Details)
                ON CONFLICT ON CONSTRAINT ""PK_ThingUpdates"" DO UPDATE
                SET
                    ""UpdateTimestamp"" = EXCLUDED.""UpdateTimestamp"",
                    ""Title""           = EXCLUDED.""Title"",
                    ""Details""         = EXCLUDED.""Details"";
            ",
            thingIdsParam, categoriesParam, tsParam, titlesParam, detailsParam
        );
    }
}
