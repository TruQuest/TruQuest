using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__LotteryExpired")]
public class ThingAssessmentVerifierLottery__LotteryExpiredError
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; set; }
}