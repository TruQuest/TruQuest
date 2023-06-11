using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("castVote")]
public class CastAssessmentPollVoteMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("uint16", "_proposalVerifiersArrayIndex", 2)]
    public ushort ProposalVerifiersArrayIndex { get; init; }
    [Parameter("uint8", "_vote", 3)]
    public Vote Vote { get; init; }
}