using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.AcceptancePoll;

[Event("PollFinalized")]
public class PollFinalizedEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("uint8", "decision", 2, false)]
    public int Decision { get; set; }
    [Parameter("string", "voteAggIpfsCid", 3, false)]
    public string VoteAggIpfsCid { get; set; }
    [Parameter("address", "submitter", 4, false)]
    public string Submitter { get; set; }
    [Parameter("address[]", "rewardedVerifiers", 5, false)]
    public List<string> RewardedVerifiers { get; set; }
    [Parameter("address[]", "slashedVerifiers", 6, false)]
    public List<string> SlashedVerifiers { get; set; }
}