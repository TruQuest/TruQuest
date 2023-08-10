using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("getParticipants", "address[]")]
public class GetThingAssessmentVerifierLotteryParticipantsMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
}
