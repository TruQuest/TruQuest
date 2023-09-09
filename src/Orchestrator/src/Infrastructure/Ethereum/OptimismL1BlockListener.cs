using System.Runtime.CompilerServices;

using Microsoft.Extensions.Configuration;

using Nethereum.Web3;

using Application.Common.Interfaces;

using Infrastructure.Ethereum.Messages;

namespace Infrastructure.Ethereum;

internal class OptimismL1BlockListener : IBlockListener
{
    private readonly Web3 _web3;
    private readonly string _l1BlockContractAddress;

    public OptimismL1BlockListener(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"]!;
        _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:URL"]);
        _l1BlockContractAddress = configuration[$"Ethereum:Contracts:{network}:L1Block:Address"]!;
    }

    public async IAsyncEnumerable<long> GetNext([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        long lastReportedBlockNumber = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            var blockNumber = (long)await _web3.Eth
                .GetContractQueryHandler<GetOptimismL1BlockNumberMessage>()
                .QueryAsync<ulong>(_l1BlockContractAddress, new());

            // @@NOTE: Don't need block confirmations here since in BlockTracker we wait til
            // all L2 events corresponding to this L1 block are processed and for an event to be
            // considered processed it has to be confirmed first.

            if (blockNumber > lastReportedBlockNumber)
            {
                lastReportedBlockNumber = blockNumber;
                yield return blockNumber;
            }

            await Task.Delay(TimeSpan.FromSeconds(2)); // @@TODO: Config.
        }
    }
}
