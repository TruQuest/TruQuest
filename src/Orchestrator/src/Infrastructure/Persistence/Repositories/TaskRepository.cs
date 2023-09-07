using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class TaskRepository : Repository, ITaskRepository
{
    private new readonly AppDbContext _dbContext;

    public TaskRepository(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(DeferredTask task) => _dbContext.Tasks.Add(task);

    public async Task SetCompletedStateFor(long taskId)
    {
        var idParam = new NpgsqlParameter<long>("Id", NpgsqlDbType.Bigint)
        {
            TypedValue = taskId
        };

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                UPDATE truquest.""Tasks""
                SET ""ScheduledBlockNumber"" = -1
                WHERE ""Id"" = @Id;
            ",
            idParam
        );
    }
}
