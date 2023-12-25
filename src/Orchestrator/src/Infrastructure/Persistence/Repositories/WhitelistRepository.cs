using Domain.Aggregates;

namespace Infrastructure.Persistence.Repositories;

internal class WhitelistRepository : Repository, IWhitelistRepository
{
    private readonly new AppDbContext _dbContext;

    public WhitelistRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(WhitelistEntry entry) => _dbContext.Whitelist.Add(entry);

    public void Remove(WhitelistEntry entry) => _dbContext.Whitelist.Remove(entry);
}
