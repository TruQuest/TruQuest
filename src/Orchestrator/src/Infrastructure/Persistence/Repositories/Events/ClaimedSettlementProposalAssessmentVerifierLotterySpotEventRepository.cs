using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class ClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository :
    Repository,
    IClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository
{
    private new readonly EventDbContext _dbContext;

    public ClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository(
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent @event)
    {
        _dbContext.ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents.Add(@event);
    }

    public Task<List<ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent>> FindAllFor(
        Guid thingId, Guid proposalId
    ) => _dbContext.ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents
        .AsNoTracking()
        .Where(e => e.ThingId == thingId && e.SettlementProposalId == proposalId)
        .OrderBy(e => e.BlockNumber)
            .ThenBy(e => e.TxnIndex)
        .ToListAsync();

    public async Task UpdateNoncesFor(IEnumerable<ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent> events)
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
                UPDATE truquest_events.""ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents"" AS c
                SET ""Nonce"" = e.""Nonce""
                FROM ""EventIdToNonce"" AS e
                WHERE c.""Id"" = e.""Id""
            ",
            eventIdsParam, noncesParam
        );
    }
}
