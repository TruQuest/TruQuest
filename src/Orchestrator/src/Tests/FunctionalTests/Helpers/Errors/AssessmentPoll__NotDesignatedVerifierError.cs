using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("AssessmentPoll__NotDesignatedVerifier")]
public class AssessmentPoll__NotDesignatedVerifierError
{
    [Parameter("bytes32", "combinedId", 1)]
    public byte[] CombinedId { get; set; }
}