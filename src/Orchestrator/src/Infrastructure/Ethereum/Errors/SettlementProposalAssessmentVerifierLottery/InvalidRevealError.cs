using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Errors.SettlementProposalAssessmentVerifierLottery;

[Error("SettlementProposalAssessmentVerifierLottery__InvalidReveal")]
public class InvalidRevealError : BaseError
{
    [Parameter("bytes32", "thingProposalId", 1)]
    public byte[] ThingProposalId { get; set; }
}
