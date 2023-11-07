using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates.Events;

namespace Infrastructure.Persistence.Repositories.Events;

internal class JoinedThingValidationVerifierLotteryEventRepository :
    Repository,
    IJoinedThingValidationVerifierLotteryEventRepository
{
    private new readonly EventDbContext _dbContext;

    public JoinedThingValidationVerifierLotteryEventRepository(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(JoinedThingValidationVerifierLotteryEvent @event) =>
        _dbContext.JoinedThingValidationVerifierLotteryEvents.Add(@event);

    public Task<List<JoinedThingValidationVerifierLotteryEvent>> FindAllFor(Guid thingId)
    {
        return _dbContext.JoinedThingValidationVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId)
            .OrderBy(e => e.BlockNumber)
                .ThenBy(e => e.TxnIndex)
                    .ThenBy(e => e.LogIndex)
            .ToListAsync();
    }

    public async Task UpdateNoncesFor(IEnumerable<JoinedThingValidationVerifierLotteryEvent> events)
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
                UPDATE truquest_events.""JoinedThingValidationVerifierLotteryEvents"" AS j
                SET ""Nonce"" = e.""Nonce""
                FROM ""EventIdToNonce"" AS e
                WHERE j.""Id"" = e.""Id""
            ",
            eventIdsParam, noncesParam
        );
    }
}
