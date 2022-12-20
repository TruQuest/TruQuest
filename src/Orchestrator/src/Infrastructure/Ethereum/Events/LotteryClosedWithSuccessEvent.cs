using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events;

[Event("LotteryClosedWithSuccess")]
public class LotteryClosedWithSuccessEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("address", "orchestrator", 2, false)]
    public string Orchestrator { get; set; }
    [Parameter("uint256", "nonce", 3, false)]
    public BigInteger Nonce { get; set; }
    [Parameter("address[]", "winners", 4, false)]
    public List<string> WinnerIds { get; set; }
}