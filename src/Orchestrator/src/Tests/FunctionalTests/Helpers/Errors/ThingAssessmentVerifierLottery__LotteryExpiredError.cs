using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__LotteryExpired")]
public class ThingAssessmentVerifierLottery__LotteryExpiredError
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; set; }
}