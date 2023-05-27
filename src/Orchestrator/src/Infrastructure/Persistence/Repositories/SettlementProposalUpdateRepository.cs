using Microsoft.Extensions.Configuration;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class SettlementProposalUpdateRepository :
    Repository<SettlementProposalUpdate>, ISettlementProposalUpdateRepository
{
    public SettlementProposalUpdateRepository(
        IConfiguration configuration,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, sharedTxnScope) { }

    public async Task AddOrUpdate(params SettlementProposalUpdate[] updateEvents)
    {
        using var cmd = await CreateCommand(
            @"
                INSERT INTO truquest.""SettlementProposalUpdates"" (
                    ""SettlementProposalId"", ""Category"", ""UpdateTimestamp"", ""Title"", ""Details""
                )
                SELECT *
                FROM UNNEST(@SettlementProposalIds, @Categories, @UpdateTimestamps, @Titles, @Details)
                ON CONFLICT ON CONSTRAINT ""PK_SettlementProposalUpdates"" DO UPDATE
                SET
                    ""UpdateTimestamp"" = EXCLUDED.""UpdateTimestamp"",
                    ""Title""           = EXCLUDED.""Title"",
                    ""Details""         = EXCLUDED.""Details"";
            "
        );
        cmd.Parameters.AddRange(new NpgsqlParameter[] {
            new NpgsqlParameter<Guid[]>("SettlementProposalIds", NpgsqlDbType.Uuid | NpgsqlDbType.Array)
            {
                TypedValue = updateEvents.Select(e => e.SettlementProposalId).ToArray()
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