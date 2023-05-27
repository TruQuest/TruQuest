using Microsoft.Extensions.Configuration;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class ThingUpdateRepository : Repository<ThingUpdate>, IThingUpdateRepository
{
    public ThingUpdateRepository(
        IConfiguration configuration,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, sharedTxnScope) { }

    public async Task AddOrUpdate(params ThingUpdate[] updateEvents)
    {
        using var cmd = await CreateCommand(
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
            "
        );
        cmd.Parameters.AddRange(new NpgsqlParameter[] {
            new NpgsqlParameter<Guid[]>("ThingIds", NpgsqlDbType.Uuid | NpgsqlDbType.Array)
            {
                TypedValue = updateEvents.Select(e => e.ThingId).ToArray()
            },
            new NpgsqlParameter<int[]>("Categories", NpgsqlDbType.Integer | NpgsqlDbType.Array)
            {
                TypedValue = updateEvents.Select(e => (int)e.Category).ToArray()
            },
            new NpgsqlParameter<long[]>("UpdateTimestamps", NpgsqlDbType.Bigint | NpgsqlDbType.Array)
            {
                TypedValue = updateEvents.Select(e => e.UpdateTimestamp).ToArray()
            },
            new NpgsqlParameter<string[]>("Titles", NpgsqlDbType.Text | NpgsqlDbType.Array)
            {
                TypedValue = updateEvents.Select(e => e.Title).ToArray()
            },
            new NpgsqlParameter<string?[]>("Details", NpgsqlDbType.Text | NpgsqlDbType.Array)
            {
                TypedValue = updateEvents.Select(e => e.Details).ToArray()
            }
        });

        await cmd.ExecuteNonQueryAsync();
    }
}