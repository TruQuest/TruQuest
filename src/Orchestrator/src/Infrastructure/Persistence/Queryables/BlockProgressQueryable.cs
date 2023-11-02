using Microsoft.EntityFrameworkCore;

using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class BlockProgressQueryable : Queryable, IBlockProgressQueryable
{
    private new readonly EventDbContext _dbContext;

    public BlockProgressQueryable(EventDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<long?> GetLastProcessedBlock() =>
        _dbContext.BlockProcessedEvent.Select(e => e.BlockNumber).SingleAsync();
}
