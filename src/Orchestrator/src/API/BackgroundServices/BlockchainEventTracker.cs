using MediatR;

using Application.Common.Interfaces;
using Application.Ethereum.Events.BlockMined;

namespace API.BackgroundServices;

public class BlockchainEventTracker : BackgroundService
{
    private readonly ILogger<BlockchainEventTracker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBlockTracker _blockTracker;

    public BlockchainEventTracker(
        ILogger<BlockchainEventTracker> logger,
        IServiceProvider serviceProvider,
        IBlockTracker blockTracker
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _blockTracker = blockTracker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (long blockNumber in _blockTracker.GetNext(stoppingToken))
        {
            _logger.LogInformation("Current block {BlockNumber}", blockNumber);

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();
            await mediator.Publish(new BlockMinedEvent { BlockNumber = blockNumber });
        }
    }
}