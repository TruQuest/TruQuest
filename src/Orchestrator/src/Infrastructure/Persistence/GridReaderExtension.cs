using Dapper;

namespace Infrastructure.Persistence;

internal static class GridReaderExtension
{
    public static TRoot? SingleWithMany<TRoot, TJoined>(
        this SqlMapper.GridReader reader,
        Func<TRoot, ICollection<TJoined>> joinedCollectionSelector,
        bool buffered = true,
        string splitOn = "Id"
    )
    {
        var roots = new HashSet<TRoot>();

        reader.Read<TRoot, TJoined, TRoot>(
            (root, joined) =>
            {
                TRoot? cachedRoot;
                if (!roots.TryGetValue(root, out cachedRoot))
                {
                    roots.Add(root);
                    cachedRoot = root;
                }
                // @@TODO: Check what dapper does if joined1 is NULL.
                if (joined != null)
                {
                    ICollection<TJoined> joinedCollection = joinedCollectionSelector(cachedRoot);
                    joinedCollection.Add(joined);
                }

                return cachedRoot;
            },
            splitOn, buffered
        );

        return roots.SingleOrDefault();
    }

    public static TRoot? SingleWithMultipleMany<TRoot, TJoined1, TJoined2>(
        this SqlMapper.GridReader reader,
        Func<TRoot, ICollection<TJoined1>> joined1CollectionSelector,
        Func<TRoot, ICollection<TJoined2>> joined2CollectionSelector,
        bool buffered = true,
        string splitOn = "Id"
    )
    {
        var roots = new HashSet<TRoot>();

        reader.Read<TRoot, TJoined1, TJoined2, TRoot>(
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
            splitOn, buffered
        );

        return roots.SingleOrDefault();
    }
}