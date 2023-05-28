using System.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;

namespace Infrastructure.Persistence.Queryables;

public abstract class Queryable : IDisposable
{
    private readonly string? _dbConnectionString;
    private NpgsqlConnection? _dbConnection;

    private readonly DbContext? _dbContext;

    protected Queryable(IConfiguration configuration)
    {
        _dbConnectionString = configuration.GetConnectionString("Postgres")!;
    }

    protected Queryable(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Dispose() => _dbConnection?.Dispose();

    protected async ValueTask<NpgsqlConnection> _getOpenConnection()
    {
        if (_dbContext != null)
        {
            var dbConn = _dbContext.Database.GetDbConnection();
            if (dbConn.State != ConnectionState.Open)
            {
                await _dbContext.Database.OpenConnectionAsync();
            }
            return (NpgsqlConnection)dbConn;
        }
        if (_dbConnection == null)
        {
            _dbConnection = new NpgsqlConnection(_dbConnectionString);
        }
        if (_dbConnection.State != ConnectionState.Open)
        {
            await _dbConnection.OpenAsync();
        }

        return _dbConnection;
    }
}