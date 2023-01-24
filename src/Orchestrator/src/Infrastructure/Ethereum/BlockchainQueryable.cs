using Microsoft.Extensions.Configuration;

using Nethereum.Hex.HexTypes;
using Nethereum.Web3;

using Application.Common.Interfaces;

namespace Infrastructure.Ethereum;

internal class BlockchainQueryable : IBlockchainQueryable
{
    private readonly Web3 _web3;

    public BlockchainQueryable(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"]!;
        _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:URL"]);
    }

    public async Task<long> GetBlockTimestamp(long blockNumber)
    {
        var block = await _web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(
            new HexBigInteger(blockNumber)
        );
        return (long)block.Timestamp.Value * 1000;
    }
}