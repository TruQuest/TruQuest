using System.Data;
using System.Data.Common;

using Microsoft.EntityFrameworkCore;

using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal abstract class Queryable
{
    protected readonly DbContext _dbContext;

    protected Queryable(DbContext dbContext, ISharedTxnScope sharedTxnScope)
    {
        _dbContext = dbContext;
        if (sharedTxnScope.DbConnection != null)
        {
            _dbContext.Database.SetDbConnection(sharedTxnScope.DbConnection);
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
