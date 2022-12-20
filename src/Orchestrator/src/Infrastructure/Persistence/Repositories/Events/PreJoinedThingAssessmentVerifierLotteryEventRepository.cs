using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;
using NpgsqlTypes;
using Nethereum.Util;

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
        var thingIdHashParam = new NpgsqlParameter<string>("ThingIdHash", NpgsqlDbType.Text)
        {
            TypedValue = Sha3Keccack.Current.CalculateHash(thingId.ToString())
        };
        var settlementProposalIdHashParam = new NpgsqlParameter<string>("SettlementProposalIdHash", NpgsqlDbType.Text)
        {
            TypedValue = Sha3Keccack.Current.CalculateHash(settlementProposalId.ToString())
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
                            @{thingIdHashParam.ParameterName},
                            @{settlementProposalIdHashParam.ParameterName},
                            @{winnerIdsParam.ParameterName}
                        )
                    ",
                    thingIdHashParam, settlementProposalIdHashParam, winnerIdsParam
                )
                .ToListAsync();
    }
}