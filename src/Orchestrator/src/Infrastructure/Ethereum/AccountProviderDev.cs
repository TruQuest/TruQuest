using Microsoft.Extensions.Configuration;

using Nethereum.HdWallet;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3.Accounts;

namespace Infrastructure.Ethereum;

internal class AccountProviderDev : IAccountProvider
{
    private readonly Dictionary<string, int> _accountNameToIndex;
    private readonly Wallet _wallet;
    private readonly Dictionary<string, string> _addressToAccountName = new();

    public AccountProviderDev(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"]!;
        _wallet = new Wallet(configuration["Ethereum:Mnemonic"], seedPassword: null);
        _accountNameToIndex = configuration.GetSection($"Ethereum:Accounts:{network}").Get<Dictionary<string, int>>()!;

        var addresses = _wallet.GetAddresses(_accountNameToIndex.Values.Max() + 1);
        foreach (var (accountName, index) in _accountNameToIndex)
        {
            _addressToAccountName[addresses[index].RemoveHexPrefix().ToLower()] = accountName;
        }
    }

    public Account GetAccount(string name) => _wallet.GetAccount(_accountNameToIndex[name]);
}
