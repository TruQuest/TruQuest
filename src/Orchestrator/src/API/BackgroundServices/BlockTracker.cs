using GoThataway;

using Application.Common.Interfaces;
using Application.Ethereum.Events.BlockMined;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace API.BackgroundServices;

public class BlockTracker : BackgroundService
{
    private readonly ILogger<BlockTracker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBlockListener _blockListener;
    private readonly IL2BlockchainQueryable _l2BlockchainQueryable;

    public BlockTracker(
        ILogger<BlockTracker> logger,
        IServiceProvider serviceProvider,
        IBlockListener blockListener,
        IL2BlockchainQueryable l2BlockchainQueryable
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _blockListener = blockListener;
        _l2BlockchainQueryable = l2BlockchainQueryable;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (long latestL1block in _blockListener.GetNext(stoppingToken))
        {
            _logger.LogDebug($"Latest confirmed L1 block: {BlockNum}", latestL1block);

            using var scope = _serviceProvider.CreateScope();
            var blockProgressQueryable = scope.ServiceProvider.GetRequiredService<IBlockProgressQueryable>();

            var allEventsCorrespondingToLatestL1BlockAreProcessed = false;
            do
            {
                long? lastProcessedL2Block = await blockProgressQueryable.GetLastProcessedBlock();
                if (lastProcessedL2Block == null) break;

                long correspondingL1Block = await _l2BlockchainQueryable.GetCorrespondingL1BlockNumberFor(
                    lastProcessedL2Block.Value
                );

                if (correspondingL1Block > latestL1block)
                {
                    // @@NOTE: Since an L2 block is marked as processed only when /all/ of its events are processed,
                    // if we get here it means that there won't be any more L2 events corresponding to 'latestL1blockNumber'.
                    // From now on only events corresponding to ('latestL1blockNumber' + 1) and later are possible, which
                    // means that 'latestL1blockNumber' can safely be considered fully processed.
                    allEventsCorrespondingToLatestL1BlockAreProcessed = true;
                }
                else
                {
                    _logger.LogInformation(
                        $"Waiting for all L2 events corresponding to L1 block {BlockNum} to be processed",
                        latestL1block
                    );
                    await Task.Delay(TimeSpan.FromSeconds(2)); // @@TODO: Config.
                }
            } while (!allEventsCorrespondingToLatestL1BlockAreProcessed);

            if (!allEventsCorrespondingToLatestL1BlockAreProcessed) continue;

            var thataway = scope.ServiceProvider.GetRequiredService<Thataway>();
            await thataway.Dispatch(new BlockMinedEvent { BlockNumber = latestL1block });
        }
    }
}
