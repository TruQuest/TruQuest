using System.Data;
using System.Transactions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;

using Domain.Base;

namespace Infrastructure.Persistence.Repositories;

internal abstract class Repository<T> : IRepository<T> where T : IAggregateRoot
{
    private readonly string _dbConnectionString;
    private readonly DbContext? _dbContext;
    private NpgsqlConnection? _dbConnection;

    private bool _useSharedDbConnection;
    private TransactionScope? _txnScope;

    private Repository(IConfiguration configuration)
    {
        _dbConnectionString = configuration.GetConnectionString("Postgres")!;
    }

    protected Repository(IConfiguration configuration, SharedTxnScope sharedTxnScope) : this(configuration)
    {
        if (sharedTxnScope.DbConnection != null && !sharedTxnScope.ExcludeRepos!.Contains(GetType()))
        {
            _setDbConnection(sharedTxnScope.DbConnection);
            _useSharedDbConnection = true;
        }
    }

    protected Repository(IConfiguration configuration, DbContext dbContext, SharedTxnScope sharedTxnScope) : this(configuration)
    {
        _dbContext = dbContext;
        if (sharedTxnScope.DbConnection != null && !sharedTxnScope.ExcludeRepos!.Contains(GetType()))
        {
            _setDbConnection(sharedTxnScope.DbConnection);
            _useSharedDbConnection = true;
        }
    }

    private void _setDbConnection(NpgsqlConnection dbConnection)
    {
        if (_dbContext != null)
        {
            _dbContext.Database.SetDbConnection(dbConnection);
        }
        else
        {
            _dbConnection = dbConnection;
        }
    }

    public virtual async ValueTask SaveChanges()
    {
        if (_dbContext != null)
        {
            if (!_useSharedDbConnection)
            {
                using var txnScope = new TransactionScope(
                    TransactionScopeOption.Suppress,
                    TransactionScopeAsyncFlowOption.Enabled
                );
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                await _dbContext.SaveChangesAsync();
            }
        }
        else
        {
            if (!_useSharedDbConnection)
            {
                _txnScope!.Complete();
                _txnScope.Dispose();
                _txnScope = null;
            }
        }
    }

    private async ValueTask<NpgsqlConnection> _ensureConnectionOpen()
    {
        if (_dbConnection == null)
        {
            _dbConnection = new NpgsqlConnection(_dbConnectionString);
        }
        if (_dbConnection.State != ConnectionState.Open)
        {
            if (!_useSharedDbConnection)
            {
                _txnScope = new TransactionScope(
                    TransactionScopeOption.RequiresNew,
                    new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Serializable },
                    TransactionScopeAsyncFlowOption.Enabled
                );
            }
            await _dbConnection.OpenAsync();
        }

        return _dbConnection;
    }

    protected async ValueTask<NpgsqlCommand> CreateCommand(
        string commandText,
        CommandType commandType = CommandType.Text
    ) => new NpgsqlCommand
    {
        Connection = await _ensureConnectionOpen(),
        CommandText = commandText,
        CommandType = commandType
    };

    void IDisposable.Dispose()
    {
        if (!_useSharedDbConnection && _dbConnection != null)
        {
            _txnScope?.Dispose();
            _dbConnection.Dispose();
        }
    }
}