using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingAssessmentVerifierLottery;

[Event("PreJoinedLottery")]
public class PreJoinedLotteryEvent : IEventDTO
{
    [Parameter("string", "thingId", 1, true)]
    public string ThingIdHash { get; set; }
    [Parameter("string", "settlementProposalId", 2, true)]
    public string SettlementProposalIdHash { get; set; }
    [Parameter("address", "user", 3, true)]
    public string UserId { get; set; }
    [Parameter("bytes32", "dataHash", 4, false)]
    public byte[] DataHash { get; set; }
}