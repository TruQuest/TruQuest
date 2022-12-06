using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class ThingRepository : Repository<Thing>, IThingRepository
{
    private readonly AppDbContext _dbContext;

    public ThingRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(Thing thing)
    {
        _dbContext.Things.Add(thing);
    }

    public Task<Thing> FindByIdHash(string idHash) => _dbContext.Things.SingleAsync(t => t.IdHash == idHash);

    public Task<Thing> FindById(Guid id) => _dbContext.Things.SingleAsync(t => t.Id == id);

    public async Task<bool> CheckIsVerifierFor(Guid thingId, string userId)
    {
        var thing = await _dbContext.Things
            .AsNoTracking()
            .Include(t => t.Verifiers.Where(v => v.VerifierId == userId))
            .Where(t => t.Id == thingId)
            .SingleAsync();

        return thing.Verifiers.Any();
    }

    public async Task<IReadOnlyList<ThingVerifier>> GetAllVerifiersFor(Guid thingId)
    {
        var thing = await _dbContext.Things
            .AsNoTracking()
            .Include(t => t.Verifiers)
            .SingleAsync(t => t.Id == thingId);

        return thing.Verifiers;
    }
}