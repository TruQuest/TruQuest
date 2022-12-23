using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignedAcceptancePollVoteAggTd")]
public class SignedAcceptancePollVoteAggTd
{
    [Parameter("tuple[]", "offChainVotes", 1, "OffChainAcceptancePollVoteTd[]")]
    public List<OffChainAcceptancePollVoteTd> OffChainVotes { get; init; }
    [Parameter("tuple[]", "onChainVotes", 2, "OnChainAcceptancePollVoteTd[]")]
    public List<OnChainAcceptancePollVoteTd> OnChainVotes { get; init; }
}