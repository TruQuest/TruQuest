using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class TaskQueryable : Queryable, ITaskQueryable
{
    private new readonly AppDbContext _dbContext;

    public TaskQueryable(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<DeferredTask>> GetAllWithScheduledBlockNumber(long leBlockNumber) =>
        _dbContext.Tasks
            .AsNoTracking()
            .Where(t => t.ScheduledBlockNumber > 0 && t.ScheduledBlockNumber <= leBlockNumber)
            .ToListAsync();
}
