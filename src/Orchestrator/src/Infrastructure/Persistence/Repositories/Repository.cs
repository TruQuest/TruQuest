using System.Data.Common;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal abstract class Repository
{
    protected readonly DbContext _dbContext;

    protected Repository(DbContext dbContext)
    {
        _dbContext = dbContext;
        if (DbConnectionProvider.TryGet(out DbConnection? dbConn))
        {
            _dbContext.Database.SetDbConnection(dbConn!);
        }
    }

    public virtual Task<int> SaveChanges() => _dbContext.SaveChangesAsync();
}
