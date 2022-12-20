using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("claimLotterySpot")]
public class ClaimLotterySpotMessage : FunctionMessage
{
    [Parameter("string", "_thingId", 1)]
    public string ThingId { get; init; }
}