using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events.SettlementProposalAssessmentVerifierLottery;

[Event("LotteryClosedInFailure")]
public class LotteryClosedInFailureEvent : IEventDTO
{
    [Parameter("bytes16", "thingId", 1, true)]
    public byte[] ThingId { get; set; }
    [Parameter("bytes16", "settlementProposalId", 2, true)]
    public byte[] SettlementProposalId { get; set; }
    [Parameter("uint8", "requiredNumVerifiers", 3, false)]
    public int RequiredNumVerifiers { get; set; }
    [Parameter("uint8", "joinedNumVerifiers", 4, false)]
    public int JoinedNumVerifiers { get; set; }
}
