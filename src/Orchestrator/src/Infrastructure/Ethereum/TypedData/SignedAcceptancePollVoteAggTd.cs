using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignedAcceptancePollVoteAggTd")]
public class SignedAcceptancePollVoteAggTd
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; init; }
    [Parameter("tuple[]", "offChainVotes", 2, "OffChainAcceptancePollVoteTd[]")]
    public List<OffChainAcceptancePollVoteTd> OffChainVotes { get; init; }
    [Parameter("tuple[]", "onChainVotes", 3, "OnChainAcceptancePollVoteTd[]")]
    public List<OnChainAcceptancePollVoteTd> OnChainVotes { get; init; }
}