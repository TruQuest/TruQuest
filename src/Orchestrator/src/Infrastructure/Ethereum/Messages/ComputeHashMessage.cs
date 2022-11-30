using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("computeHash", "bytes32")]
public class ComputeHashMessage : FunctionMessage
{
    [Parameter("bytes32", "_data", 1)]
    public byte[] Data { get; init; }
}