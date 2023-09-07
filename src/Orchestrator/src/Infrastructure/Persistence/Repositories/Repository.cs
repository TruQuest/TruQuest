using Microsoft.EntityFrameworkCore;

using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal abstract class Repository
{
    protected readonly DbContext _dbContext;

    protected Repository(DbContext dbContext, ISharedTxnScope sharedTxnScope)
    {
        _dbContext = dbContext;
        if (sharedTxnScope.DbConnection != null)
        {
            _dbContext.Database.SetDbConnection(sharedTxnScope.DbConnection);
        }
    }

    public virtual Task<int> SaveChanges() => _dbContext.SaveChangesAsync();
}
