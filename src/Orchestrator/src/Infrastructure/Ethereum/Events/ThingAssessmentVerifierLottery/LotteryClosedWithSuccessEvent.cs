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
    [Parameter("uint256", "nonce", 3, false)]
    public BigInteger Nonce { get; set; }
    [Parameter("address[]", "claimants", 4, false)]
    public List<string> ClaimantIds { get; set; }
    [Parameter("address[]", "winners", 5, false)]
    public List<string> WinnerIds { get; set; }
}