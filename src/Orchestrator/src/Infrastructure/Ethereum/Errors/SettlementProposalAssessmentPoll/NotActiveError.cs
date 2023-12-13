using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Errors.SettlementProposalAssessmentPoll;

[Error("SettlementProposalAssessmentPoll__NotActive")]
public class NotActiveError : BaseError
{
    [Parameter("bytes32", "thingProposalId", 1)]
    public byte[] ThingProposalId { get; set; }
}
