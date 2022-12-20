using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__NotCommittedToLottery")]
public class ThingAssessmentVerifierLottery__NotCommittedToLotteryError
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; set; }
}