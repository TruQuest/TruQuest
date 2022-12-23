using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignedAssessmentPollVoteAggTd")]
public class SignedAssessmentPollVoteAggTd
{
    [Parameter("tuple[]", "offChainVotes", 1, "OffChainAssessmentPollVoteTd[]")]
    public List<OffChainAssessmentPollVoteTd> OffChainVotes { get; init; }
    [Parameter("tuple[]", "onChainVotes", 2, "OnChainAssessmentPollVoteTd[]")]
    public List<OnChainAssessmentPollVoteTd> OnChainVotes { get; init; }
}