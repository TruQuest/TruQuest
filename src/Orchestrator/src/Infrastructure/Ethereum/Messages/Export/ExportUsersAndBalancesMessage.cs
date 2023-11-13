using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages.Export;

[Function("exportUsersAndBalances", typeof(ExportUsersAndBalancesFunctionOutput))]
public class ExportUsersAndBalancesMessage : FunctionMessage { }

[FunctionOutput]
public class ExportUsersAndBalancesFunctionOutput
{
    [Parameter("address[]", "users", 1)]
    public List<string> Users { get; set; }
    [Parameter("uint256[]", "balances", 2)]
    public List<BigInteger> Balances { get; set; }
    [Parameter("uint256[]", "stakedBalances", 3)]
    public List<BigInteger> StakedBalances { get; set; }
}
