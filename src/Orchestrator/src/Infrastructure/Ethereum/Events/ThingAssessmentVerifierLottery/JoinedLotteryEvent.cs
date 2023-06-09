using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingAssessmentVerifierLottery;

[Event("JoinedLottery")]
public class JoinedLotteryEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("bytes16", "settlementProposalId", 2, true)]
    public byte[] SettlementProposalId { get; set; }
    [Parameter("address", "user", 3, true)]
    public string UserId { get; set; }
    [Parameter("bytes32", "userData", 4, false)]
    public byte[] UserData { get; set; }
    [Parameter("uint256", "l1BlockNumber", 5, false)]
    public BigInteger L1BlockNumber { get; set; }
}