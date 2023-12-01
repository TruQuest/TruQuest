using Microsoft.Extensions.Configuration;

using Nethereum.Web3.Accounts;

namespace Infrastructure.Ethereum;

internal class AccountProvider : IAccountProvider
{
    private readonly Account _orchestrator;

    public AccountProvider(IConfiguration configuration)
    {
        _orchestrator = new Account(configuration["Ethereum:OrchestratorPrivateKey"]);
    }

    public Account GetAccount(string name)
    {
        if (name != "Orchestrator") throw new InvalidOperationException();
        return _orchestrator;
    }
}
