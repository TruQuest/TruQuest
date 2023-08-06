using Microsoft.Extensions.Configuration;

using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Hex.HexConvertors.Extensions;

using Application.Common.Interfaces;

namespace Infrastructure.Ethereum;

internal class L1BlockchainQueryable : IL1BlockchainQueryable
{
    private readonly Web3 _web3;

    public L1BlockchainQueryable(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"]!;
        if (network == "Ganache")
        {
            _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:URL"]);
        }
        else
        {
            _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:SettlementNetwork:URL"]);
        }
    }

    public async Task<byte[]> GetBlockHash(long blockNumber)
    {
        var block = await _web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(
            new HexBigInteger(blockNumber)
        );
        return block.BlockHash.HexToByteArray();
    }

    public async Task<long> GetBlockTimestamp(long blockNumber)
    {
        var block = await _web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(
            new HexBigInteger(blockNumber)
        );
        return (long)block.Timestamp.Value * 1000;
    }
}
