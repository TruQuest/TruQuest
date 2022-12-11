using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("transfer", "bool")]
public class TransferMessage : FunctionMessage
{
    [Parameter("address", "to", 1)]
    public string To { get; init; }
    [Parameter("uint256", "amount", 2)]
    public BigInteger Amount { get; init; }
}