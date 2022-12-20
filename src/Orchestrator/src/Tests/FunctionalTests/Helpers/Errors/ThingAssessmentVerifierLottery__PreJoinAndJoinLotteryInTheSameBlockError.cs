using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__PreJoinAndJoinLotteryInTheSameBlock")]
public class ThingAssessmentVerifierLottery__PreJoinAndJoinLotteryInTheSameBlockError
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; set; }
}