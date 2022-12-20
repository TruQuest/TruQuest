using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__LotteryNotActive")]
public class ThingAssessmentVerifierLottery__LotteryNotActiveError
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; set; }
}