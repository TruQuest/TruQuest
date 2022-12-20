using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__InvalidLotteryReveal")]
public class ThingAssessmentVerifierLottery__InvalidLotteryRevealError
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; set; }
}