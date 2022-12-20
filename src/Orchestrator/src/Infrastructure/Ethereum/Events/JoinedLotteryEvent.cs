using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events;

[Event("JoinedLottery")]
public class JoinedLotteryEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("address", "user", 2, true)]
    public string UserId { get; set; }
    [Parameter("uint256", "nonce", 3, false)]
    public BigInteger Nonce { get; set; }
}