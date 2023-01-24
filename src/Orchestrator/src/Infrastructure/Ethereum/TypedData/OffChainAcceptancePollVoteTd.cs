using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("OffChainAcceptancePollVoteTd")]
public class OffChainAcceptancePollVoteTd
{
    [Parameter("string", "ipfsCid", 1)]
    public string IpfsCid { get; init; }
}