using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class SettlementProposalUpdateRepository : Repository, ISettlementProposalUpdateRepository
{
    public SettlementProposalUpdateRepository(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope) { }

    public async Task AddOrUpdate(params SettlementProposalUpdate[] updateEvents)
    {
        var proposalIdsParam = new NpgsqlParameter<Guid[]>("SettlementProposalIds", NpgsqlDbType.Uuid | NpgsqlDbType.Array)
        {
            TypedValue = updateEvents.Select(e => e.SettlementProposalId).ToArray()
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
            ",
            proposalIdsParam, categoriesParam, tsParam, titlesParam, detailsParam
        );
    }
}
