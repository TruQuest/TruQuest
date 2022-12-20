using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__AlreadyCommittedToLottery")]
public class ThingAssessmentVerifierLottery__AlreadyCommittedToLotteryError
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; set; }
}