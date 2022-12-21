using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;
using NpgsqlTypes;

using Domain.QM;
using Domain.Aggregates.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories.Events;

internal class PreJoinedThingSubmissionVerifierLotteryEventRepository : Repository<PreJoinedThingSubmissionVerifierLotteryEvent>, IPreJoinedThingSubmissionVerifierLotteryEventRepository
{
    private readonly EventDbContext _dbContext;

    public PreJoinedThingSubmissionVerifierLotteryEventRepository(
        IConfiguration configuration,
        EventDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(PreJoinedThingSubmissionVerifierLotteryEvent @event)
    {
        _dbContext.PreJoinedThingSubmissionVerifierLotteryEvents.Add(@event);
    }

    public Task<List<VerifierLotteryWinnerQm>> GetLotteryWinnerIndices(Guid thingId, IEnumerable<string> winnerIds)
    {
        var thingIdParam = new NpgsqlParameter<Guid>("ThingId", NpgsqlDbType.Uuid)
        {
            TypedValue = thingId
        };
        var winnerIdsParam = new NpgsqlParameter<string[]>("WinnerIds", NpgsqlDbType.Text | NpgsqlDbType.Array)
        {
            TypedValue = winnerIds.ToArray()
        };

        return _dbContext.VerifierLotteryWinners
                .FromSqlRaw(
                    $@"
                        SELECT *
                        FROM ""SelectWinnerIndicesAccordingToPreJoinedThingSubmissionVerifierLotteryEvents"" (
                            @{thingIdParam.ParameterName}, @{winnerIdsParam.ParameterName}
                        )
                    ",
                    thingIdParam, winnerIdsParam
                )
                .ToListAsync();
    }
}