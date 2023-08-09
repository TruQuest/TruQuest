using Application.Common.Interfaces;
using Application.Ethereum.Events.BlockMined;
using Infrastructure;

namespace API.BackgroundServices;

public class BlockTracker : BackgroundService
{
    private readonly ILogger<BlockTracker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBlockListener _blockListener;

    public BlockTracker(
        ILogger<BlockTracker> logger,
        IServiceProvider serviceProvider,
        IBlockListener blockListener
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _blockListener = blockListener;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (long blockNumber in _blockListener.GetNext(stoppingToken))
        {
            _logger.LogInformation("Latest confirmed L1 block: {BlockNumber}", blockNumber);

            // @@TODO!!: Check that BlockProcessedEvent.BlockNumber's epoch > blockNumber.
            // If not wait til it is.

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<PublisherWrapper>();

            await mediator.Publish(
                new BlockMinedEvent
                {
                    BlockNumber = blockNumber
                },
                addToAdditionalSinks: false
            );
        }
    }
}
