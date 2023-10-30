using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("closeLotteryInFailure")]
public class CloseSettlementProposalAssessmentVerifierLotteryInFailureMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("uint8", "_joinedNumVerifiers", 2)]
    public int JoinedNumVerifiers { get; init; }
}
