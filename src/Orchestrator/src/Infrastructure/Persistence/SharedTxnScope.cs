using System.Data.Common;

using Microsoft.Extensions.Configuration;

using Npgsql;

using Application.Common.Interfaces;

namespace Infrastructure.Persistence;

public class SharedTxnScope : ISharedTxnScope
{
    private readonly string _dbConnectionString;
    public DbConnection? DbConnection { get; private set; }
    public HashSet<Type>? ExcludeRepos { get; private set; }

    public SharedTxnScope(IConfiguration configuration)
    {
        _dbConnectionString = configuration.GetConnectionString("Postgres")!;
    }

    public void Init(Type[]? excludeRepos)
    {
        DbConnection = new NpgsqlConnection(_dbConnectionString);
        ExcludeRepos = new(excludeRepos ?? new Type[] { });
    }

    void IDisposable.Dispose()
    {
        DbConnection?.Dispose();
    }
}