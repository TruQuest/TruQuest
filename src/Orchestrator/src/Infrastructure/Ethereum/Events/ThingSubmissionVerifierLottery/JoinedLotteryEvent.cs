using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingSubmissionVerifierLottery;

[Event("JoinedLottery")]
public class JoinedLotteryEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("address", "user", 2, true)]
    public string UserId { get; set; }
    [Parameter("bytes32", "userData", 3, false)]
    public byte[] UserData { get; set; }
}