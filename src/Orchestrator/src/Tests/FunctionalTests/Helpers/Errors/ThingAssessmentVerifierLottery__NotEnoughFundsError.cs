using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__NotEnoughFunds")]
public class ThingAssessmentVerifierLottery__NotEnoughFundsError
{
    [Parameter("uint256", "requiredFunds", 1)]
    public BigInteger RequiredFunds { get; set; }
}