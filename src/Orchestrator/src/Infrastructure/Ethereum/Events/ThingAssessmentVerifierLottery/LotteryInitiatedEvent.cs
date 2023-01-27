using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingAssessmentVerifierLottery;

[Event("LotteryInitiated")]
public class LotteryInitiatedEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("bytes16", "settlementProposalId", 2, true)]
    public byte[] SettlementProposalId { get; set; }
    [Parameter("address", "orchestrator", 3, false)]
    public string Orchestrator { get; set; }
    [Parameter("bytes32", "dataHash", 4, false)]
    public byte[] DataHash { get; set; }
}