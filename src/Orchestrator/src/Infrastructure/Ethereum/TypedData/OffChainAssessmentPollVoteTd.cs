using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("OffChainAssessmentPollVoteTd")]
public class OffChainAssessmentPollVoteTd
{
    [Parameter("string", "ipfsCid", 1)]
    public string IpfsCid { get; init; }
}