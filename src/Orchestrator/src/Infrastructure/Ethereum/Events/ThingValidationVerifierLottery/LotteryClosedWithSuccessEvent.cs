using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingValidationVerifierLottery;

[Event("LotteryClosedWithSuccess")]
public class LotteryClosedWithSuccessEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("address", "orchestrator", 2, false)]
    public string Orchestrator { get; set; }
    [Parameter("bytes32", "data", 3, false)]
    public byte[] Data { get; set; }
    [Parameter("bytes32", "userXorData", 4, false)]
    public byte[] UserXorData { get; set; }
    [Parameter("bytes32", "hashOfL1EndBlock", 5, false)]
    public byte[] HashOfL1EndBlock { get; set; }
    [Parameter("uint256", "nonce", 6, false)]
    public BigInteger Nonce { get; set; }
    [Parameter("address[]", "winners", 7, false)]
    public List<string> Winners { get; set; }
}
