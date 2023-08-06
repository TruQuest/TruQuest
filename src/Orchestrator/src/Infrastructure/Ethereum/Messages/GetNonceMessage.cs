using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("getNonce", "uint256")]
public class GetNonceMessage : FunctionMessage
{
    [Parameter("address", "sender", 1)]
    public string Sender { get; init; }
    [Parameter("uint192", "key", 2)]
    public BigInteger Key { get; init; }
}
