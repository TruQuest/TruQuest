using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;
using NpgsqlTypes;
using Nethereum.Util;

using Domain.QM;
using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class PreJoinedLotteryEventRepository : Repository<PreJoinedLotteryEvent>, IPreJoinedLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public PreJoinedLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(PreJoinedLotteryEvent @event)
    {
        _dbContext.PreJoinedLotteryEvents.Add(@event);
    }

    public Task<List<LotteryWinnerQm>> GetLotteryWinnerIndices(Guid thingId, IEnumerable<string> winnerIds)
    {
        var thingIdHashParam = new NpgsqlParameter<string>("ThingIdHash", NpgsqlDbType.Text)
        {
            TypedValue = Sha3Keccack.Current.CalculateHash(thingId.ToString())
        };
        var winnerIdsParam = new NpgsqlParameter<string[]>("WinnerIds", NpgsqlDbType.Text | NpgsqlDbType.Array)
        {
            TypedValue = winnerIds.ToArray()
        };

        return _dbContext.LotteryWinners
                .FromSqlRaw(
                    $@"
                        SELECT *
                        FROM ""SelectWinnerIndicesAccordingToPreJoinedLotteryEvents"" (
                            @{thingIdHashParam.ParameterName}, @{winnerIdsParam.ParameterName}
                        )
                    ",
                    thingIdHashParam, winnerIdsParam
                )
                .ToListAsync();
    }
}