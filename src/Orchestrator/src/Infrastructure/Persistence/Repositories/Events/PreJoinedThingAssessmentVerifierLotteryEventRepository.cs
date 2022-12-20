using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;
using NpgsqlTypes;

using Domain.QM;
using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class PreJoinedThingAssessmentVerifierLotteryEventRepository :
    Repository<PreJoinedThingAssessmentVerifierLotteryEvent>,
    IPreJoinedThingAssessmentVerifierLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public PreJoinedThingAssessmentVerifierLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(PreJoinedThingAssessmentVerifierLotteryEvent @event)
    {
        _dbContext.PreJoinedThingAssessmentVerifierLotteryEvents.Add(@event);
    }

    public Task<List<VerifierLotteryWinnerQm>> GetLotteryWinnerIndices(
        Guid thingId, Guid settlementProposalId, IEnumerable<string> winnerIds
    )
    {
        var thingIdParam = new NpgsqlParameter<Guid>("ThingId", NpgsqlDbType.Uuid)
        {
            TypedValue = thingId
        };
        var settlementProposalIdParam = new NpgsqlParameter<Guid>("SettlementProposalId", NpgsqlDbType.Uuid)
        {
            TypedValue = settlementProposalId
        };
        var winnerIdsParam = new NpgsqlParameter<string[]>("WinnerIds", NpgsqlDbType.Text | NpgsqlDbType.Array)
        {
            TypedValue = winnerIds.ToArray()
        };

        return _dbContext.VerifierLotteryWinners
                .FromSqlRaw(
                    $@"
                        SELECT *
                        FROM ""SelectWinnerIndicesAccordingToPreJoinedThingAssessmentVerifierLotteryEvents"" (
                            @{thingIdParam.ParameterName},
                            @{settlementProposalIdParam.ParameterName},
                            @{winnerIdsParam.ParameterName}
                        )
                    ",
                    thingIdParam, settlementProposalIdParam, winnerIdsParam
                )
                .ToListAsync();
    }
}