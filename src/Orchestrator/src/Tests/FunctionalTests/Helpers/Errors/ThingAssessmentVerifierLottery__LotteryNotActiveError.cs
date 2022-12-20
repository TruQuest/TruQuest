using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__LotteryNotActive")]
public class ThingAssessmentVerifierLottery__LotteryNotActiveError
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; set; }
}