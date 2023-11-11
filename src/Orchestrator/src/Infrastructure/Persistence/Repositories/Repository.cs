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
            var dbConnToReplace = _dbContext.Database.GetDbConnection();
            if (dbConnToReplace != dbConn!)
            {
                // @@NOTE: If we have multiple repos and/or queryables that use the same context,
                // and they all get resolved at the same time (i.e. all instances are created before any
                // of them is /used/), then all's copacetic – since the connection is not yet open, we
                // can safely call SetDbConnection many times (even though we only need to call it once).
                // But this breaks if some repo/queryable that uses the same context gets resolved and used
                // earlier than all the rest (e.g. in a mediatr behavior). In such a case, since
                // mediatr handlers get resolved "just in time", when the rest get created later,
                // they try to call SetDbConnection on the context with an already open connection, which
                // is not allowed. So we check if connection from DbConnectionProvider is already set on
                // the context.
                _dbContext.Database.SetDbConnection(dbConn);
            }
        }
    }

    public virtual Task<int> SaveChanges() => _dbContext.SaveChangesAsync();
}
