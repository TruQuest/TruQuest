using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("claimLotterySpot")]
public class ClaimLotterySpotMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
}