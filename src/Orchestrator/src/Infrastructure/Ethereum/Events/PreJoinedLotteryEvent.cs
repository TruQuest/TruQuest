using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events;

[Event("PreJoinedLottery")]
public class PreJoinedLotteryEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("address", "user", 2, true)]
    public string UserId { get; set; }
    [Parameter("bytes32", "dataHash", 3, false)]
    public byte[] DataHash { get; set; }
}