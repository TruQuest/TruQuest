using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Errors.SettlementProposalAssessmentVerifierLottery;

[Error("SettlementProposalAssessmentVerifierLottery__StillInProgress")]
public class StillInProgressError : BaseError
{
    [Parameter("bytes32", "thingProposalId", 1)]
    public byte[] ThingProposalId { get; set; }
}
