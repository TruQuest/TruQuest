using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.SettlementProposalAssessmentVerifierLottery;

[Event("LotteryInitialized")]
public class LotteryInitializedEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("bytes16", "settlementProposalId", 2, true)]
    public byte[] SettlementProposalId { get; set; }
    [Parameter("uint256", "l1BlockNumber", 3, false)]
    public BigInteger L1BlockNumber { get; set; }
    [Parameter("address", "orchestrator", 4, false)]
    public string Orchestrator { get; set; }
    [Parameter("bytes32", "dataHash", 5, false)]
    public byte[] DataHash { get; set; }
    [Parameter("bytes32", "userXorDataHash", 6, false)]
    public byte[] UserXorDataHash { get; set; }
}
