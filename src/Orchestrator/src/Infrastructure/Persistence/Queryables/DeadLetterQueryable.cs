using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class DeadLetterQueryable : Queryable, IDeadLetterQueryable
{
    private readonly new AppDbContext _dbContext;

    public DeadLetterQueryable(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> GetUnhandledCount() => _dbContext.DeadLetters.CountAsync(l => l.State == DeadLetterState.Unhandled);
}
