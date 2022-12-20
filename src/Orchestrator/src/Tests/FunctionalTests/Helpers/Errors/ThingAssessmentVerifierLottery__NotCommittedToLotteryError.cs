using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__NotCommittedToLottery")]
public class ThingAssessmentVerifierLottery__NotCommittedToLotteryError
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; set; }
}