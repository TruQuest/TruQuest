using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;
using NpgsqlTypes;
using Nethereum.Util;

using Domain.QM;
using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class PreJoinedVerifierLotteryEventRepository : Repository<PreJoinedVerifierLotteryEvent>, IPreJoinedVerifierLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public PreJoinedVerifierLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(PreJoinedVerifierLotteryEvent @event)
    {
        _dbContext.PreJoinedVerifierLotteryEvents.Add(@event);
    }

    public Task<List<VerifierLotteryWinnerQm>> GetLotteryWinnerIndices(Guid thingId, IEnumerable<string> winnerIds)
    {
        var thingIdHashParam = new NpgsqlParameter<string>("ThingIdHash", NpgsqlDbType.Text)
        {
            TypedValue = Sha3Keccack.Current.CalculateHash(thingId.ToString())
        };
        var winnerIdsParam = new NpgsqlParameter<string[]>("WinnerIds", NpgsqlDbType.Text | NpgsqlDbType.Array)
        {
            TypedValue = winnerIds.ToArray()
        };

        return _dbContext.VerifierLotteryWinners
                .FromSqlRaw(
                    $@"
                        SELECT *
                        FROM ""SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents"" (
                            @{thingIdHashParam.ParameterName}, @{winnerIdsParam.ParameterName}
                        )
                    ",
                    thingIdHashParam, winnerIdsParam
                )
                .ToListAsync();
    }
}