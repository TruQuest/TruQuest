using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignedAssessmentPollVoteAggTd")]
public class SignedAssessmentPollVoteAggTd
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; init; }
    [Parameter("string", "settlementProposalId", 2)]
    public string SettlementProposalId { get; init; }
    [Parameter("tuple[]", "offChainVotes", 3, "OffChainAssessmentPollVoteTd[]")]
    public List<OffChainAssessmentPollVoteTd> OffChainVotes { get; init; }
    [Parameter("tuple[]", "onChainVotes", 4, "OnChainAssessmentPollVoteTd[]")]
    public List<OnChainAssessmentPollVoteTd> OnChainVotes { get; init; }
}