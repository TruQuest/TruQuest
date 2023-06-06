using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class JoinedThingSubmissionVerifierLotteryEventRepository :
    Repository<JoinedThingSubmissionVerifierLotteryEvent>, IJoinedThingSubmissionVerifierLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public JoinedThingSubmissionVerifierLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(JoinedThingSubmissionVerifierLotteryEvent @event)
    {
        _dbContext.JoinedThingSubmissionVerifierLotteryEvents.Add(@event);
    }

    public Task<List<JoinedThingSubmissionVerifierLotteryEvent>> FindAllFor(Guid thingId)
    {
        return _dbContext.JoinedThingSubmissionVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId)
            .OrderBy(e => e.BlockNumber)
                .ThenBy(e => e.TxnIndex)
            .ToListAsync();
    }

    public async Task UpdateNoncesFor(IEnumerable<JoinedThingSubmissionVerifierLotteryEvent> events)
    {
        var eventIdsParam = new NpgsqlParameter<long[]>("EventIds", NpgsqlDbType.Bigint | NpgsqlDbType.Array)
        {
            TypedValue = events.Select(e => e.Id!.Value).ToArray()
        };
        var noncesParam = new NpgsqlParameter<long[]>("Nonces", NpgsqlDbType.Bigint | NpgsqlDbType.Array)
        {
            TypedValue = events.Select(e => e.Nonce!.Value).ToArray()
        };

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                WITH ""EventIdToNonce"" (""Id"", ""Nonce"") AS (
                    SELECT *
                    FROM UNNEST(@EventIds, @Nonces)
                )
                UPDATE truquest_events.""JoinedThingSubmissionVerifierLotteryEvents"" AS j
                SET ""Nonce"" = e.""Nonce""
                FROM ""EventIdToNonce"" AS e
                WHERE j.""Id"" = e.""Id""
            ",
            eventIdsParam, noncesParam
        );
    }
}