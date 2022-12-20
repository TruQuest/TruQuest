using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__AlreadyCommittedToLottery")]
public class ThingAssessmentVerifierLottery__AlreadyCommittedToLotteryError
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; set; }
}