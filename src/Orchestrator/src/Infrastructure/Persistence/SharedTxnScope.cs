using Microsoft.Extensions.Configuration;

using Npgsql;

namespace Infrastructure.Persistence;

public class SharedTxnScope : IDisposable
{
    private readonly string _dbConnectionString;
    public NpgsqlConnection? DbConnection { get; private set; }
    public HashSet<Type>? ExcludeRepos { get; private set; }

    public SharedTxnScope(IConfiguration configuration)
    {
        _dbConnectionString = configuration.GetConnectionString("Postgres")!;
    }

    public void Init()
    {
        DbConnection = new(_dbConnectionString);
        ExcludeRepos = new();
    }

    void IDisposable.Dispose()
    {
        DbConnection?.Dispose();
    }
}