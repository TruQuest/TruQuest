using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("execute")]
public class ExecuteMessage : FunctionMessage
{
    [Parameter("address", "dest", 1)]
    public string Dest { get; init; }
    [Parameter("uint256", "value", 2)]
    public BigInteger Value { get; init; }
    [Parameter("bytes", "func", 3)]
    public byte[] Func { get; init; }
}
