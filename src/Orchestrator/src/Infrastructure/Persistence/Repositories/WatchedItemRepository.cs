using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;
using Application.Common.Interfaces;
using Application.User.Commands.MarkNotificationsAsRead;

namespace Infrastructure.Persistence.Repositories;

internal class WatchedItemRepository : Repository<WatchedItem>, IWatchedItemRepository
{
    private readonly AppDbContext _dbContext;

    public WatchedItemRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Add(WatchedItem watchedItem)
    {
        _dbContext.WatchList.Add(watchedItem);
    }

    public void Remove(WatchedItem watchedItem)
    {
        _dbContext.WatchList.Remove(watchedItem);
    }

    public async Task UpdateLastSeenTimestamp(IEnumerable<WatchedItem> watchedItems)
    {
        var itemTypesParam = new NpgsqlParameter<int[]>("ItemTypes", NpgsqlDbType.Integer | NpgsqlDbType.Array)
        {
            TypedValue = watchedItems.Select(i => (int)i.ItemType).ToArray()
        };
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