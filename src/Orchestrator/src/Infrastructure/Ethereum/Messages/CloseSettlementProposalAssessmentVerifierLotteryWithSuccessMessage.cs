using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("closeLotteryWithSuccess")]
public class CloseSettlementProposalAssessmentVerifierLotteryWithSuccessMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("bytes32", "_data", 2)]
    public byte[] Data { get; init; }
    [Parameter("bytes32", "_userXorData", 3)]
    public byte[] UserXorData { get; init; }
    [Parameter("bytes32", "_hashOfL1EndBlock", 4)]
    public byte[] HashOfL1EndBlock { get; init; }
    [Parameter("uint64[]", "_winnerClaimantIndices", 5)]
    public List<ulong> WinnerClaimantIndices { get; init; }
    [Parameter("uint64[]", "_winnerIndices", 6)]
    public List<ulong> WinnerIndices { get; init; }
}
