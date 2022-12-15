using Nethereum.Web3;
using Nethereum.Web3.Accounts;

public class TestContract : IAsyncLifetime
{
    public string Address { get; private set; }

    public async Task InitializeAsync()
    {
        var account = new Account("0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80");
        var web3 = new Web3(account, "http://localhost:8545/");

        var txnReceipt = await web3.Eth.GetContractDeploymentHandler<TestContractDeploymentMessage>()
            .SendRequestAndWaitForReceiptAsync(new());

        Address = txnReceipt.ContractAddress;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}