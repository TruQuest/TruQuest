using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingAssessmentVerifierLottery;

[Event("LotteryClosedWithSuccess")]
public class LotteryClosedWithSuccessEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("bytes16", "settlementProposalId", 2, true)]
    public byte[] SettlementProposalId { get; set; }
    [Parameter("address", "orchestrator", 3, false)]
    public string Orchestrator { get; set; }
    [Parameter("bytes32", "data", 4, false)]
    public byte[] Data { get; set; }
    [Parameter("bytes32", "userXorData", 5, false)]
    public byte[] UserXorData { get; set; }
    [Parameter("bytes32", "hashOfL1EndBlock", 6, false)]
    public byte[] HashOfL1EndBlock { get; set; }
    [Parameter("uint256", "nonce", 7, false)]
    public BigInteger Nonce { get; set; }
    [Parameter("address[]", "claimants", 8, false)]
    public List<string> ClaimantIds { get; set; }
    [Parameter("address[]", "winners", 9, false)]
    public List<string> WinnerIds { get; set; }
}