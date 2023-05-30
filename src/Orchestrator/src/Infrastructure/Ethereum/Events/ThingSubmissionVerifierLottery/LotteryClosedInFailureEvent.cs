using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingSubmissionVerifierLottery;

[Event("LotteryClosedInFailure")]
public class LotteryClosedInFailureEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("uint8", "requiredNumVerifiers", 2, false)]
    public int RequiredNumVerifiers { get; set; }
    [Parameter("uint8", "joinedNumVerifiers", 3, false)]
    public int JoinedNumVerifiers { get; set; }
}