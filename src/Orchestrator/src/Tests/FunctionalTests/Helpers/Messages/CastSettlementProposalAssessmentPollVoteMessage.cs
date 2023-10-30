using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("castVote")]
public class CastSettlementProposalAssessmentPollVoteMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("uint16", "_settlementProposalVerifiersArrayIndex", 2)]
    public ushort SettlementProposalVerifiersArrayIndex { get; init; }
    [Parameter("uint8", "_vote", 3)]
    public Vote Vote { get; init; }
}
