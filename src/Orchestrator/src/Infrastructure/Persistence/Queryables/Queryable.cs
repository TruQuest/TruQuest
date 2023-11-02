using System.Data;
using System.Data.Common;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Queryables;

internal abstract class Queryable
{
    protected readonly DbContext _dbContext;

    protected Queryable(DbContext dbContext)
    {
        _dbContext = dbContext;
        if (DbConnectionProvider.TryGet(out DbConnection? dbConn))
        {
            _dbContext.Database.SetDbConnection(dbConn!);
        }
    }

    protected async ValueTask<DbConnection> _getOpenConnection()
    {
        var dbConn = _dbContext.Database.GetDbConnection();
        if (dbConn.State != ConnectionState.Open)
        {
            await _dbContext.Database.OpenConnectionAsync();
        }

        return dbConn;
    }
}
