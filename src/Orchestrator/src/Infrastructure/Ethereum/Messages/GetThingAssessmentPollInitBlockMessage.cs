using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("getPollInitBlock", "uint256")]
public class GetThingAssessmentPollInitBlockMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
}