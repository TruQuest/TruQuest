using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignedAssessmentPollVoteAggTd")]
public class SignedAssessmentPollVoteAggTd
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; init; }
    [Parameter("string", "settlementProposalId", 2)]
    public string SettlementProposalId { get; init; }
    [Parameter("uint64", "endBlock", 3)]
    public ulong EndBlock { get; init; }
    [Parameter("tuple[]", "offChainVotes", 4, "OffChainAssessmentPollVoteTd[]")]
    public List<OffChainAssessmentPollVoteTd> OffChainVotes { get; init; }
    [Parameter("tuple[]", "onChainVotes", 5, "OnChainAssessmentPollVoteTd[]")]
    public List<OnChainAssessmentPollVoteTd> OnChainVotes { get; init; }
}