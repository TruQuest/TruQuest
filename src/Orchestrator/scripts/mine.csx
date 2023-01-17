#r "nuget: Nethereum.Web3, 4.11.0"

using Nethereum.JsonRpc.Client;
using Nethereum.Web3;

int numBlocks = int.Parse(Args[0]);

var web3 = new Web3("http://localhost:7545/");
for (int i = 0; i < numBlocks; ++i)
{
    await web3.Client.SendRequestAsync(new RpcRequest(Guid.NewGuid().ToString(), "evm_mine"));
}