using Microsoft.EntityFrameworkCore;

using Dapper;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class WhitelistQueryable : Queryable, IWhitelistQueryable
{
    private readonly new AppDbContext _dbContext;

    public WhitelistQueryable(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CheckIsWhitelisted(WhitelistEntryType entryType, string value) =>
        await _dbContext.Whitelist.CountAsync(e => e.Type == entryType && e.Value == value) > 0;

    public async Task<bool> CheckIsWhitelisted(string userId)
    {
        var dbConn = await _getOpenConnection();
        return await dbConn.QueryFirstOrDefaultAsync<int?>(
            @"
                WITH ""UserEntry"" AS (
                    SELECT ""Email"", ""UserName""
                    FROM truquest.""AspNetUsers""
                    WHERE ""Id"" = @UserId
                )
                SELECT 1
                FROM
                    truquest.""Whitelist"" AS w
                        INNER JOIN
                    ""UserEntry"" AS u
                        ON (
                            w.""Type"" = 'email'::truquest.whitelist_entry_type AND w.""Value"" = u.""Email""
                                OR
                            w.""Type"" = 'signer_address'::truquest.whitelist_entry_type AND w.""Value"" = u.""UserName""
                        );
            ",
            param: new { UserId = userId }
        ) != null;
    }
}
