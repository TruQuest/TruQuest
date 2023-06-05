using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingSubmissionVerifierLottery;

[Event("LotteryInitialized")]
public class LotteryInitializedEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("address", "orchestrator", 2, false)]
    public string Orchestrator { get; set; }
    [Parameter("bytes32", "dataHash", 3, false)]
    public byte[] DataHash { get; set; }
    [Parameter("bytes32", "userXorDataHash", 4, false)]
    public byte[] UserXorDataHash { get; set; }
}