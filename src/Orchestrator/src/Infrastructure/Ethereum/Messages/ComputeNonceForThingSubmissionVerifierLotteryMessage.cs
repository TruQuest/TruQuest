using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("computeNonce", "uint256")]
public class ComputeNonceForThingSubmissionVerifierLotteryMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
    [Parameter("bytes32", "_data", 2)]
    public byte[] Data { get; init; }
}