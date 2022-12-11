using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("approve", "bool")]
public class ApproveMessage : FunctionMessage
{
    [Parameter("address", "spender", 1)]
    public string Spender { get; init; }
    [Parameter("uint256", "amount", 2)]
    public BigInteger Amount { get; init; }
}