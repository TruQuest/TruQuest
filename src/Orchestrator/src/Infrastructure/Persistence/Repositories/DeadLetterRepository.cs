using Domain.Aggregates;

namespace Infrastructure.Persistence.Repositories;

internal class DeadLetterRepository : Repository, IDeadLetterRepository
{
    private readonly new AppDbContext _dbContext;

    public DeadLetterRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(DeadLetter deadLetter) => _dbContext.Add(deadLetter);
}
