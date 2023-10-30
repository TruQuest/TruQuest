using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingValidationVerifierLottery;

[Event("LotteryInitialized")]
public class LotteryInitializedEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("uint256", "l1BlockNumber", 2, false)]
    public BigInteger L1BlockNumber { get; set; }
    [Parameter("address", "orchestrator", 3, false)]
    public string Orchestrator { get; set; }
    [Parameter("bytes32", "dataHash", 4, false)]
    public byte[] DataHash { get; set; }
    [Parameter("bytes32", "userXorDataHash", 5, false)]
    public byte[] UserXorDataHash { get; set; }
}
