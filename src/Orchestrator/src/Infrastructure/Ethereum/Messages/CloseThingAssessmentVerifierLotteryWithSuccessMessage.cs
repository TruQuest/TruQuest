using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("closeLotteryWithSuccess")]
public class CloseThingAssessmentVerifierLotteryWithSuccessMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("bytes32", "_data", 2)]
    public byte[] Data { get; init; }
    [Parameter("uint64[]", "_winnerClaimantIndices", 3)]
    public List<ulong> WinnerClaimantIndices { get; init; }
    [Parameter("uint64[]", "_winnerIndices", 4)]
    public List<ulong> WinnerIndices { get; init; }
}