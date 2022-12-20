using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__PreJoinAndJoinLotteryInTheSameBlock")]
public class ThingAssessmentVerifierLottery__PreJoinAndJoinLotteryInTheSameBlockError
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; set; }
}