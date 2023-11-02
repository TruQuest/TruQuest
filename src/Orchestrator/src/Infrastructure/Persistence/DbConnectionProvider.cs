using System.Data.Common;

using Npgsql;

namespace Infrastructure.Persistence;

public static class DbConnectionProvider
{
    private static readonly AsyncLocal<DbConnection> _asyncLocal = new();
    public static string ConnectionString { get; set; }

    public static DbConnection Init()
    {
        return _asyncLocal.Value = new NpgsqlConnection(ConnectionString);
    }

    public static bool TryGet(out DbConnection? dbConn)
    {
        dbConn = _asyncLocal.Value;
        return dbConn != null;
    }
}
