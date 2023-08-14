using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("executeBatch")]
public class ExecuteBatchMessage : FunctionMessage
{
    [Parameter("address[]", "dest", 1)]
    public List<string> Dest { get; init; }
    [Parameter("uint256[]", "value", 2)]
    public List<BigInteger> Value { get; init; }
    [Parameter("bytes[]", "func", 3)]
    public List<byte[]> Func { get; init; }
}
