using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.ThingAssessmentVerifierLottery;

[Event("JoinedLottery")]
public class JoinedLotteryEvent : IEventDTO
{
    [Parameter("string", "thingId", 1, true)]
    public string ThingIdHash { get; set; }
    [Parameter("string", "settlementProposalId", 2, true)]
    public string SettlementProposalIdHash { get; set; }
    [Parameter("address", "user", 3, true)]
    public string UserId { get; set; }
    [Parameter("uint256", "nonce", 4, false)]
    public BigInteger Nonce { get; set; }
}