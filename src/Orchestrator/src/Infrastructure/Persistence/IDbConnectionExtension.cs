using System.Data;

using Dapper;

namespace Infrastructure.Persistence;

internal static class IDbConnectionExtension
{
    public static async Task<IEnumerable<TRoot>> QueryWithMany<TRoot, TJoined, TRootKey>(
        this IDbConnection dbConn,
        string sql,
        Func<TRoot, TRootKey> rootKeySelector,
        Func<TRoot, IList<TJoined>> joinedCollectionSelector,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null
    ) where TRootKey : notnull
    {
        var cache = new Dictionary<TRootKey, TRoot>();

        await dbConn.QueryAsync<TRoot, TJoined, TRoot>(
            sql,
            (root, joined) =>
            {
                var rootKey = rootKeySelector(root);
                if (!cache.ContainsKey(rootKey))
                {
                    cache.Add(rootKey, root);
                }

                TRoot cachedRoot = cache[rootKey];
                if (joined != null)
                {
                    IList<TJoined> joinedCollection = joinedCollectionSelector(cachedRoot);
                    joinedCollection.Add(joined);
                }

                return cachedRoot;
            },
            param, transaction, buffered, splitOn, commandTimeout, commandType
        );

        return cache.Values;
    }

    public static async Task<TRoot?> SingleWithMany<TRoot, TJoined, TRootKey>(
        this IDbConnection dbConn,
        string sql,
        Func<TRoot, TRootKey> rootKeySelector,
        Func<TRoot, IList<TJoined>> joinedCollectionSelector,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null
    ) where TRootKey : notnull
    {
        var cache = new Dictionary<TRootKey, TRoot>();

        await dbConn.QueryAsync<TRoot, TJoined, TRoot>(
            sql,
            (root, joined) =>
            {
                var rootKey = rootKeySelector(root);
                if (!cache.ContainsKey(rootKey))
                {
                    cache.Add(rootKey, root);
                }

                TRoot cachedRoot = cache[rootKey];
                if (joined != null)
                {
                    IList<TJoined> joinedCollection = joinedCollectionSelector(cachedRoot);
                    joinedCollection.Add(joined);
                }

                return cachedRoot;
            },
            param, transaction, buffered, splitOn, commandTimeout, commandType
        );

        return cache.Values.SingleOrDefault();
    }
}