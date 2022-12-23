using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.AcceptancePoll;

[Event("CastedVoteWithReason")]
public class CastedVoteWithReasonEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("address", "user", 2, true)]
    public string UserId { get; set; }
    [Parameter("uint8", "vote", 3, false)]
    public int Vote { get; set; }
    [Parameter("string", "reason", 4, false)]
    public string Reason { get; set; }
}