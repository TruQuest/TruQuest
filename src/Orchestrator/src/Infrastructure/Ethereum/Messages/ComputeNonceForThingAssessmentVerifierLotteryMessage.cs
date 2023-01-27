using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("computeNonce", "uint256")]
public class ComputeNonceForThingAssessmentVerifierLotteryMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("bytes32", "_data", 2)]
    public byte[] Data { get; init; }
}