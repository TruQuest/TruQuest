using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class TaskRepository : Repository<DeferredTask>, ITaskRepository
{
    private readonly AppDbContext _dbContext;

    public TaskRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(DeferredTask task)
    {
        _dbContext.Tasks.Add(task);
    }

    public Task<List<DeferredTask>> FindAllWithScheduledBlockNumber(long leBlockNumber) =>
        _dbContext.Tasks
            .Where(t => t.ScheduledBlockNumber > 0 && t.ScheduledBlockNumber <= leBlockNumber)
            .ToListAsync();
}
