using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("SettlementProposalAssessmentPoll__NotDesignatedVerifier")]
public class SettlementProposalAssessmentPoll__NotDesignatedVerifierError
{
    [Parameter("bytes32", "thingProposalId", 1)]
    public byte[] ThingProposalId { get; set; }
}
