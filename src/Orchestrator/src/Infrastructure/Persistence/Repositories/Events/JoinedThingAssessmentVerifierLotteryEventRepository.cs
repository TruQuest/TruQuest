using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class JoinedThingAssessmentVerifierLotteryEventRepository :
    Repository<JoinedThingAssessmentVerifierLotteryEvent>,
    IJoinedThingAssessmentVerifierLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public JoinedThingAssessmentVerifierLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(JoinedThingAssessmentVerifierLotteryEvent @event)
    {
        _dbContext.JoinedThingAssessmentVerifierLotteryEvents.Add(@event);
    }

    public Task<List<JoinedThingAssessmentVerifierLotteryEvent>> FindAllFor(Guid thingId, Guid proposalId)
    {
        return _dbContext.JoinedThingAssessmentVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.SettlementProposalId == proposalId)
            .OrderBy(e => e.BlockNumber)
                .ThenBy(e => e.TxnIndex)
            .ToListAsync();
    }

    public async Task UpdateNoncesFor(IEnumerable<JoinedThingAssessmentVerifierLotteryEvent> events)
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
                UPDATE truquest_events.""JoinedThingAssessmentVerifierLotteryEvents"" AS j
                SET ""Nonce"" = e.""Nonce""
                FROM ""EventIdToNonce"" AS e
                WHERE j.""Id"" = e.""Id""
            ",
            eventIdsParam, noncesParam
        );
    }
}