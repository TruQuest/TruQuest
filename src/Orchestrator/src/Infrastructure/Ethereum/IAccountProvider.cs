using Nethereum.Web3.Accounts;

namespace Infrastructure.Ethereum;

public interface IAccountProvider
{
    Account GetAccount(string name);
}
