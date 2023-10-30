using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingValidationPoll;

[Event("CastedVote")]
public class CastedVoteEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("address", "user", 2, true)]
    public string User { get; set; }
    [Parameter("uint8", "vote", 3, false)]
    public int Vote { get; set; }
    [Parameter("uint256", "l1BlockNumber", 4, false)]
    public BigInteger L1BlockNumber { get; set; }
}
