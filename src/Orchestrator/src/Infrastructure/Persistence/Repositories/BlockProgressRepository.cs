using System.Numerics;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Nethereum.BlockchainProcessing.ProgressRepositories;

using Domain.Aggregates.Events;

namespace Infrastructure.Persistence.Repositories;

internal class BlockProgressRepository : IBlockProgressRepository
{
    private readonly IServiceProvider _serviceProvider;

    public BlockProgressRepository(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<BigInteger?> GetLastBlockNumberProcessedAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        var @event = await dbContext.BlockProcessedEvent.AsNoTracking().SingleAsync();

        return @event.BlockNumber;
    }

    public async Task UpsertProgressAsync(BigInteger blockNumber)
    {
        var @event = new BlockProcessedEvent(id: 1, blockNumber: (long)blockNumber);

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        dbContext.Update(@event);

        await dbContext.SaveChangesAsync();
    }
}