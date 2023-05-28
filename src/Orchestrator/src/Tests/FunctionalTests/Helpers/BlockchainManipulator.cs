using Microsoft.Extensions.Configuration;

using Nethereum.JsonRpc.Client;
using Nethereum.Web3;

namespace Tests.FunctionalTests.Helpers;

public class BlockchainManipulator
{
    private readonly Web3 _web3;

    public BlockchainManipulator(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"]!;
        _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:URL"]);
    }

    public async Task Mine(int numBlocks)
    {
        for (int i = 0; i < numBlocks; ++i)
        {
            await _web3.Client.SendRequestAsync(new RpcRequest(Guid.NewGuid().ToString(), "evm_mine"));
            await Task.Delay(50);
        }
    }
}