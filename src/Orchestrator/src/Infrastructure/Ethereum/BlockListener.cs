using System.Runtime.CompilerServices;

using Microsoft.Extensions.Configuration;

using Nethereum.Web3;

using Application.Common.Interfaces;

namespace Infrastructure.Ethereum;

internal class BlockListener : IBlockListener
{
    private readonly Web3 _web3;
    private readonly int _blockConfirmations;

    public BlockListener(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"]!;
        _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:URL"]);
        _blockConfirmations = configuration.GetValue<int>($"Ethereum:Networks:{network}:BlockConfirmations");
    }

    public async IAsyncEnumerable<long> GetNext([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        long lastReportedBlockNumber = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            var blockNumber = (long)(await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync()).Value -
                (_blockConfirmations + 1);
            if (blockNumber > lastReportedBlockNumber)
            {
                lastReportedBlockNumber = blockNumber;
                yield return blockNumber;
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}