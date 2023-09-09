using System.Numerics;

using Microsoft.Extensions.Configuration;

using Nethereum.Web3;
using Nethereum.RPC.Eth;
using Nethereum.RPC.Eth.DTOs;

using Application.Common.Interfaces;

using Infrastructure.Ethereum.Messages;

namespace Infrastructure.Ethereum;

internal class L2BlockchainQueryable : IL2BlockchainQueryable
{
    private readonly Web3 _web3;
    private readonly string _l1BlockContractAddress;

    public L2BlockchainQueryable(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"]!;
        _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:URL"]);
        _l1BlockContractAddress = configuration[$"Ethereum:Contracts:{network}:L1Block:Address"]!;
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

    public async Task<long> GetCorrespondingL1BlockNumberFor(long l2Block) =>
        (long)await _web3.Eth
            .GetContractQueryHandler<GetOptimismL1BlockNumberMessage>()
            .QueryAsync<ulong>(
                _l1BlockContractAddress,
                new(),
                new BlockParameter((ulong)l2Block) // @@NOTE: The state right /after/ all txns of this block are executed.
            );
}
