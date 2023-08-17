using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ThingAssessmentVerifierLotterySpotClaimedEventRepository :
    Repository<ThingAssessmentVerifierLotterySpotClaimedEvent>,
    IThingAssessmentVerifierLotterySpotClaimedEventRepository
{
    private readonly EventDbContext _dbContext;

    public ThingAssessmentVerifierLotterySpotClaimedEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(ThingAssessmentVerifierLotterySpotClaimedEvent @event)
    {
        _dbContext.ThingAssessmentVerifierLotterySpotClaimedEvents.Add(@event);
    }

    public Task<List<ThingAssessmentVerifierLotterySpotClaimedEvent>> FindAllFor(
        Guid thingId, Guid settlementProposalId
    ) => _dbContext.ThingAssessmentVerifierLotterySpotClaimedEvents
        .AsNoTracking()
        .Where(e => e.ThingId == thingId && e.SettlementProposalId == settlementProposalId)
        .OrderBy(e => e.BlockNumber)
            .ThenBy(e => e.TxnIndex)
        .ToListAsync();

    public async Task UpdateUserDataAndNoncesFor(IEnumerable<ThingAssessmentVerifierLotterySpotClaimedEvent> events)
    {
        var eventIdsParam = new NpgsqlParameter<long[]>("EventIds", NpgsqlDbType.Bigint | NpgsqlDbType.Array)
        {
            TypedValue = events.Select(e => e.Id!.Value).ToArray()
        };
        var userDataArrayParam = new NpgsqlParameter<string[]>("UserDataArray", NpgsqlDbType.Text | NpgsqlDbType.Array)
        {
            TypedValue = events.Select(e => e.UserData!).ToArray()
        };
        var noncesParam = new NpgsqlParameter<long[]>("Nonces", NpgsqlDbType.Bigint | NpgsqlDbType.Array)
        {
            TypedValue = events.Select(e => e.Nonce!.Value).ToArray()
        };

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                WITH ""EventIdToData"" (""Id"", ""UserData"", ""Nonce"") AS (
                    SELECT *
                    FROM UNNEST(@EventIds, @UserDataArray, @Nonces)
                )
                UPDATE truquest_events.""ThingAssessmentVerifierLotterySpotClaimedEvents"" AS c
                SET
                    ""UserData"" = e.""UserData"",
                    ""Nonce"" = e.""Nonce""
                FROM ""EventIdToData"" AS e
                WHERE c.""Id"" = e.""Id""
            ",
            eventIdsParam, userDataArrayParam, noncesParam
        );
    }
}
