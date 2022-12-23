using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingSubmissionVerifierLottery;

[Event("LotteryClosedWithSuccess")]
public class LotteryClosedWithSuccessEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("uint256", "nonce", 2, false)]
    public BigInteger Nonce { get; set; }
    [Parameter("address[]", "winners", 3, false)]
    public List<string> WinnerIds { get; set; }
}