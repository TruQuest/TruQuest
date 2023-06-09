using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("getLotteryInitBlock", "int256")]
public class GetThingAssessmentVerifierLotteryInitBlockMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
}