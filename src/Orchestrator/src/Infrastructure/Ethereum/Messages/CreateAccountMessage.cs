using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("createAccount", "address")]
public class CreateAccountMessage : FunctionMessage
{
    [Parameter("address", "owner", 1)]
    public string Owner { get; init; }
    [Parameter("uint256", "salt", 2)]
    public BigInteger Salt { get; init; }
}
