using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.SettlementProposalAssessmentVerifierLottery;

[Event("ClaimedLotterySpot")]
public class ClaimedLotterySpotEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("bytes16", "settlementProposalId", 2, true)]
    public byte[] SettlementProposalId { get; set; }
    [Parameter("address", "user", 3, true)]
    public string User { get; set; }
    [Parameter("uint256", "l1BlockNumber", 4, false)]
    public BigInteger L1BlockNumber { get; set; }
}
