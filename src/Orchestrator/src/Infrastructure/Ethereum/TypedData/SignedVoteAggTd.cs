using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignedVoteAggTd")]
public class SignedVoteAggTd
{
    [Parameter("tuple[]", "offChainVotes", 1, "OffChainVoteTd[]")]
    public List<OffChainVoteTd> OffChainVotes { get; init; }
    [Parameter("tuple[]", "onChainVotes", 2, "OnChainVoteTd[]")]
    public List<OnChainVoteTd> OnChainVotes { get; init; }
}