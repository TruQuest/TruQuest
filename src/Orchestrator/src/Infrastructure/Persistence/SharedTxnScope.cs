using System.Data.Common;

using Microsoft.Extensions.Configuration;

using Npgsql;

using Application.Common.Interfaces;

namespace Infrastructure.Persistence;

public class SharedTxnScope : ISharedTxnScope
{
    private readonly string _dbConnectionString;
    public DbConnection? DbConnection { get; private set; }

    public SharedTxnScope(IConfiguration configuration)
    {
        _dbConnectionString = configuration.GetConnectionString("Postgres")!;
    }

    public void Init()
    {
        DbConnection = new NpgsqlConnection(_dbConnectionString);
    }

    void IDisposable.Dispose() => DbConnection?.Dispose();
}
