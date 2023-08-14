using System.Numerics;

using Microsoft.Extensions.Configuration;

using Nethereum.Web3;
using Nethereum.RPC.Eth;

using Application.Common.Interfaces;

namespace Infrastructure.Ethereum;

internal class L2BlockchainQueryable : IL2BlockchainQueryable
{
    private readonly Web3 _web3;

    public L2BlockchainQueryable(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"]!;
        _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:URL"]);
    }

    public async Task<bool> CheckContractDeployed(string address)
    {
        var code = await _web3.Eth.GetCode.SendRequestAsync(address);
        return code != "0x";
    }

    public async Task<BigInteger> GetBaseFee()
    {
        var blockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        var block = await _web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(blockNumber);
        return block.BaseFeePerGas.Value;
    }

    public async Task<BigInteger> GetMaxPriorityFee()
    {
        var maxPriorityFee = await new EthMaxPriorityFeePerGas(_web3.Client).SendRequestAsync();
        return maxPriorityFee.Value;
    }
}
