using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates.Events;

namespace Infrastructure.Persistence.Repositories.Events;

internal class JoinedSettlementProposalAssessmentVerifierLotteryEventRepository :
    Repository,
    IJoinedSettlementProposalAssessmentVerifierLotteryEventRepository
{
    private new readonly EventDbContext _dbContext;

    public JoinedSettlementProposalAssessmentVerifierLotteryEventRepository(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(JoinedSettlementProposalAssessmentVerifierLotteryEvent @event)
    {
        _dbContext.JoinedSettlementProposalAssessmentVerifierLotteryEvents.Add(@event);
    }

    public Task<List<JoinedSettlementProposalAssessmentVerifierLotteryEvent>> FindAllFor(Guid thingId, Guid proposalId)
    {
        return _dbContext.JoinedSettlementProposalAssessmentVerifierLotteryEvents
            .AsNoTracking()
            .Where(e => e.ThingId == thingId && e.SettlementProposalId == proposalId)
            .OrderBy(e => e.BlockNumber)
                .ThenBy(e => e.TxnIndex)
                    .ThenBy(e => e.LogIndex)
            .ToListAsync();
    }

    public async Task UpdateNoncesFor(IEnumerable<JoinedSettlementProposalAssessmentVerifierLotteryEvent> events)
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
                UPDATE truquest_events.""JoinedSettlementProposalAssessmentVerifierLotteryEvents"" AS j
                SET ""Nonce"" = e.""Nonce""
                FROM ""EventIdToNonce"" AS e
                WHERE j.""Id"" = e.""Id""
            ",
            eventIdsParam, noncesParam
        );
    }
}
