using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__AlreadyJoinedLottery")]
public class ThingAssessmentVerifierLottery__AlreadyJoinedLotteryError
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; set; }
}