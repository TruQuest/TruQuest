using System.Numerics;

using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("mintAndDepositTruthserumTo")]
public class MintAndDepositTruthserumToMessage : FunctionMessage
{
    [Parameter("address", "_user", 1)]
    public string User { get; init; }
    [Parameter("uint256", "_amount", 2)]
    public BigInteger Amount { get; init; }
}
