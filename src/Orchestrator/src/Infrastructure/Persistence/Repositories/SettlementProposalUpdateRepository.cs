using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;
using Application.Common.Monitoring;

namespace Infrastructure.Persistence.Repositories;

internal class SettlementProposalUpdateRepository : Repository, ISettlementProposalUpdateRepository
{
    public SettlementProposalUpdateRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task AddOrUpdate(params SettlementProposalUpdate[] updateEvents)
    {
        var traceparent = Telemetry.CurrentActivity!.GetTraceparent();
        foreach (var @event in updateEvents) @event.SetTraceparent(traceparent);

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
        var traceparentsParam = new NpgsqlParameter<string?[]>("Traceparents", NpgsqlDbType.Text | NpgsqlDbType.Array)
        {
            TypedValue = updateEvents.Select(e => e.Traceparent).ToArray()
        };

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                INSERT INTO truquest.""SettlementProposalUpdates"" (
                    ""SettlementProposalId"", ""Category"", ""UpdateTimestamp"", ""Title"", ""Details"", ""Traceparent""
                )
                SELECT *
                FROM UNNEST(@SettlementProposalIds, @Categories, @UpdateTimestamps, @Titles, @Details, @Traceparents)
                ON CONFLICT ON CONSTRAINT ""PK_SettlementProposalUpdates"" DO UPDATE
                SET
                    ""UpdateTimestamp"" = EXCLUDED.""UpdateTimestamp"",
                    ""Title""           = EXCLUDED.""Title"",
                    ""Details""         = EXCLUDED.""Details"",
                    ""Traceparent""     = EXCLUDED.""Traceparent"";
            ",
            proposalIdsParam, categoriesParam, tsParam, titlesParam, detailsParam, traceparentsParam
        );
    }
}
