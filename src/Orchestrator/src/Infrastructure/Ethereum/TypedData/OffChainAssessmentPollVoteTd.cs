using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("OffChainAssessmentPollVoteTd")]
public class OffChainAssessmentPollVoteTd
{
    [Parameter("string", "settlementProposalId", 1)]
    public string SettlementProposalId { get; init; }
    [Parameter("address", "voterId", 2)]
    public string VoterId { get; init; }
    [Parameter("string", "pollType", 3)]
    public string PollType { get; init; }
    [Parameter("string", "castedAt", 4)]
    public string CastedAt { get; init; }
    [Parameter("string", "decision", 5)]
    public string Decision { get; init; }
    [Parameter("string", "reason", 6)]
    public string Reason { get; init; }
    [Parameter("string", "ipfsCid", 7)]
    public string IpfsCid { get; init; }
    [Parameter("string", "voterSignature", 8)]
    public string VoterSignature { get; init; }
}