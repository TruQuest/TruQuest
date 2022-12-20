using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__AlreadyJoinedLottery")]
public class ThingAssessmentVerifierLottery__AlreadyJoinedLotteryError
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; set; }
}