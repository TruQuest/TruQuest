using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("deposit")]
public class DepositMessage : FunctionMessage
{
    [Parameter("uint256", "_amount", 1)]
    public BigInteger Amount { get; init; }
}