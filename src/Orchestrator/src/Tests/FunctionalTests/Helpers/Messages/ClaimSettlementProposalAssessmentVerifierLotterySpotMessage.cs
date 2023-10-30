using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("claimLotterySpot")]
public class ClaimSettlementProposalAssessmentVerifierLotterySpotMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("uint16", "_thingVerifiersArrayIndex", 2)]
    public ushort ThingVerifiersArrayIndex { get; init; }
}
