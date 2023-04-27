using System.Data;

using Dapper;

namespace Infrastructure.Persistence;

internal static class IDbConnectionExtension
{
    public static async Task<IEnumerable<TRoot>> QueryWithMany<TRoot, TJoined>(
        this IDbConnection dbConn,
        string sql,
        Func<TRoot, ICollection<TJoined>> joinedCollectionSelector,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null
    )
    {
        var roots = new HashSet<TRoot>();

        await dbConn.QueryAsync<TRoot, TJoined, TRoot>(
            sql,
            (root, joined) =>
            {
                TRoot? cachedRoot;
                if (!roots.TryGetValue(root, out cachedRoot))
                {
                    roots.Add(root);
                    cachedRoot = root;
                }

                if (joined != null)
                {
                    ICollection<TJoined> joined1Collection = joinedCollectionSelector(cachedRoot);
                    joined1Collection.Add(joined);
                }

                return cachedRoot;
            },
            param, transaction, buffered, splitOn, commandTimeout, commandType
        );

        return roots;
    }

    public static async Task<TRoot?> SingleWithMany<TRoot, TJoined>(
        this IDbConnection dbConn,
        string sql,
        Func<TRoot, ICollection<TJoined>> joinedCollectionSelector,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null
    )
    {
        var roots = new HashSet<TRoot>();

        await dbConn.QueryAsync<TRoot, TJoined, TRoot>(
            sql,
            (root, joined) =>
            {
                TRoot? cachedRoot;
                if (!roots.TryGetValue(root, out cachedRoot))
                {
                    roots.Add(root);
                    cachedRoot = root;
                }

                if (joined != null)
                {
                    ICollection<TJoined> joined1Collection = joinedCollectionSelector(cachedRoot);
                    joined1Collection.Add(joined);
                }

                return cachedRoot;
            },
            param, transaction, buffered, splitOn, commandTimeout, commandType
        );

        return roots.SingleOrDefault();
    }

    public static async Task<TRoot?> SingleWithMultipleMany<TRoot, TJoined1, TJoined2>(
        this IDbConnection dbConn,
        string sql,
        Func<TRoot, ICollection<TJoined1>> joined1CollectionSelector,
        Func<TRoot, ICollection<TJoined2>> joined2CollectionSelector,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null
    )
    {
        var roots = new HashSet<TRoot>();

        await dbConn.QueryAsync<TRoot, TJoined1, TJoined2, TRoot>(
            sql,
            (root, joined1, joined2) =>
            {
                TRoot? cachedRoot;
                if (!roots.TryGetValue(root, out cachedRoot))
                {
                    roots.Add(root);
                    cachedRoot = root;
                }
                // @@TODO: Check what dapper does if joined1 is NULL.
                if (joined1 != null)
                {
                    ICollection<TJoined1> joined1Collection = joined1CollectionSelector(cachedRoot);
                    joined1Collection.Add(joined1);
                }
                if (joined2 != null)
                {
                    ICollection<TJoined2> joined2Collection = joined2CollectionSelector(cachedRoot);
                    joined2Collection.Add(joined2);
                }

                return cachedRoot;
            },
            param, transaction, buffered, splitOn, commandTimeout, commandType
        );

        return roots.SingleOrDefault();
    }
}