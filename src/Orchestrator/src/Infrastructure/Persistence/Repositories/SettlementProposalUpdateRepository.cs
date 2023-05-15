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

    public async Task AddOrUpdate(SettlementProposalUpdate updateEvent)
    {
        using var cmd = await CreateCommand(
            @"
                INSERT INTO truquest.""SettlementProposalUpdates"" (
                    ""SettlementProposalId"", ""Category"", ""UpdateTimestamp"", ""Title"", ""Details""
                )
                VALUES (@SettlementProposalId, @Category, @UpdateTimestamp, @Title, @Details)
                ON CONFLICT ON CONSTRAINT ""PK_SettlementProposalUpdates"" DO UPDATE
                SET
                    ""UpdateTimestamp"" = EXCLUDED.""UpdateTimestamp"",
                    ""Title""           = EXCLUDED.""Title"",
                    ""Details""         = EXCLUDED.""Details"";
            "
        );
        cmd.Parameters.AddRange(new NpgsqlParameter[] {
            new NpgsqlParameter<Guid>(nameof(updateEvent.SettlementProposalId), NpgsqlDbType.Uuid)
            {
                TypedValue = updateEvent.SettlementProposalId
            },
            new NpgsqlParameter<int>(nameof(updateEvent.Category), NpgsqlDbType.Integer)
            {
                TypedValue = (int)updateEvent.Category
            },
            new NpgsqlParameter<long>(nameof(updateEvent.UpdateTimestamp), NpgsqlDbType.Bigint)
            {
                TypedValue = updateEvent.UpdateTimestamp
            },
            new NpgsqlParameter<string>(nameof(updateEvent.Title), NpgsqlDbType.Text)
            {
                TypedValue = updateEvent.Title
            },
            new NpgsqlParameter<string?>(nameof(updateEvent.Details), NpgsqlDbType.Text)
            {
                TypedValue = updateEvent.Details
            }
        });

        await cmd.ExecuteNonQueryAsync();
    }
}