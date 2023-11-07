using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;

namespace Infrastructure.Persistence.Repositories;

internal class WatchedItemRepository : Repository, IWatchedItemRepository
{
    private new readonly AppDbContext _dbContext;

    public WatchedItemRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(params WatchedItem[] watchedItems) => _dbContext.WatchList.AddRange(watchedItems);

    public void Remove(WatchedItem watchedItem) => _dbContext.WatchList.Remove(watchedItem);

    public async Task DuplicateGeneralItemsFrom(WatchedItemType itemType, Guid sourceItemId, Guid destItemId)
    {
        var itemTypeParam = new NpgsqlParameter("ItemType", itemType);
        var sourceItemIdParam = new NpgsqlParameter<Guid>("SourceItemId", NpgsqlDbType.Uuid)
        {
            TypedValue = sourceItemId
        };
        var destItemIdParam = new NpgsqlParameter<Guid>("DestItemId", NpgsqlDbType.Uuid)
        {
            TypedValue = destItemId
        };
        var timestampParam = new NpgsqlParameter<long>("Timestamp", NpgsqlDbType.Bigint)
        {
            TypedValue = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                INSERT INTO truquest.""WatchList"" (
                    ""UserId"", ""ItemType"", ""ItemId"", ""ItemUpdateCategory"", ""LastSeenUpdateTimestamp""
                )
                    SELECT ""UserId"", ""ItemType"", @DestItemId, ""ItemUpdateCategory"", @Timestamp
                    FROM truquest.""WatchList""
                    WHERE
                        ""ItemType"" = @ItemType AND
                        ""ItemId"" = @SourceItemId AND
                        ""ItemUpdateCategory"" - ""ItemUpdateCategory"" / 100 * 100 = 0;
            ",
            itemTypeParam, sourceItemIdParam, destItemIdParam, timestampParam
        );
    }

    public async Task UpdateLastSeenTimestamp(IEnumerable<WatchedItem> watchedItems)
    {
        var itemTypesParam = new NpgsqlParameter("ItemTypes", watchedItems.Select(i => i.ItemType).ToArray());
        var itemIdsParam = new NpgsqlParameter<Guid[]>("ItemIds", NpgsqlDbType.Uuid | NpgsqlDbType.Array)
        {
            TypedValue = watchedItems.Select(i => i.ItemId).ToArray()
        };
        var itemUpdateCategoriesParam = new NpgsqlParameter<int[]>(
            "ItemUpdateCategories", NpgsqlDbType.Integer | NpgsqlDbType.Array
        )
        {
            TypedValue = watchedItems.Select(i => i.ItemUpdateCategory).ToArray()
        };
        var updateTimestampsParam = new NpgsqlParameter<long[]>(
            "UpdateTimestamps", NpgsqlDbType.Bigint | NpgsqlDbType.Array
        )
        {
            TypedValue = watchedItems.Select(i => i.LastSeenUpdateTimestamp).ToArray()
        };
        var userIdParam = new NpgsqlParameter<string>("UserId", NpgsqlDbType.Text)
        {
            TypedValue = watchedItems.Single().UserId
        };

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                WITH ""ItemsToUpdate"" AS (
                    SELECT *
                    FROM UNNEST(
                        @ItemTypes, @ItemIds, @ItemUpdateCategories, @UpdateTimestamps
                    ) AS vals (
                        ""ItemType"", ""ItemId"", ""ItemUpdateCategory"", ""UpdateTimestamp""
                    )
                )
                UPDATE truquest.""WatchList"" AS w
                SET ""LastSeenUpdateTimestamp"" = i.""UpdateTimestamp""
                FROM ""ItemsToUpdate"" AS i
                WHERE
                    (w.""UserId"", w.""ItemType"", w.""ItemId"", w.""ItemUpdateCategory"") =
                    (@UserId, i.""ItemType"", i.""ItemId"", i.""ItemUpdateCategory"");
            ",
            itemTypesParam,
            itemIdsParam,
            itemUpdateCategoriesParam,
            updateTimestampsParam,
            userIdParam
        );
    }
}
