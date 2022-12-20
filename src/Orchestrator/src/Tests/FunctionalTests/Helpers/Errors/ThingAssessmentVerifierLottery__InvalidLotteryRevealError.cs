using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__InvalidLotteryReveal")]
public class ThingAssessmentVerifierLottery__InvalidLotteryRevealError
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; set; }
}