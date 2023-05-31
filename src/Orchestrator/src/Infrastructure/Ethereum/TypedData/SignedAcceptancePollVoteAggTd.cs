using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignedAcceptancePollVoteAggTd")]
public class SignedAcceptancePollVoteAggTd
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; init; }
    [Parameter("uint64", "endBlock", 2)]
    public ulong EndBlock { get; init; }
    [Parameter("tuple[]", "offChainVotes", 3, "OffChainAcceptancePollVoteTd[]")]
    public List<OffChainAcceptancePollVoteTd> OffChainVotes { get; init; }
    [Parameter("tuple[]", "onChainVotes", 4, "OnChainAcceptancePollVoteTd[]")]
    public List<OnChainAcceptancePollVoteTd> OnChainVotes { get; init; }
}