using System.Data;

using Microsoft.Extensions.Configuration;

using Npgsql;

namespace Infrastructure.Persistence.Queryables;

internal abstract class Queryable : IDisposable
{
    private readonly string _dbConnectionString;
    private NpgsqlConnection? _dbConnection;

    public Queryable(IConfiguration configuration)
    {
        _dbConnectionString = configuration.GetConnectionString("Postgres") + "SearchPath=truquest;";
    }

    public void Dispose() => _dbConnection?.Dispose();

    protected async ValueTask<NpgsqlConnection> getOpenConnection()
    {
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