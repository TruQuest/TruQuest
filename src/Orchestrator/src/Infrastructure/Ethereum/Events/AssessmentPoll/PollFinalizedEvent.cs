using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.AssessmentPoll;

[Event("PollFinalized")]
public class PollFinalizedEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("bytes16", "settlementProposalId", 2, true)]
    public byte[] SettlementProposalId { get; set; }
    [Parameter("uint8", "decision", 3, false)]
    public int Decision { get; set; }
    [Parameter("string", "voteAggIpfsCid", 4, false)]
    public string VoteAggIpfsCid { get; set; }
    [Parameter("address", "submitter", 5, false)]
    public string Submitter { get; set; }
    [Parameter("address[]", "rewardedVerifiers", 6, false)]
    public List<string> RewardedVerifiers { get; set; }
    [Parameter("address[]", "slashedVerifiers", 7, false)]
    public List<string> SlashedVerifiers { get; set; }
}