using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events;

[Event("ThingFunded")]
public class ThingFundedEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("address", "user", 2, true)]
    public string User { get; set; }
    [Parameter("uint256", "thingStake", 3, false)]
    public BigInteger Stake { get; set; }
}
