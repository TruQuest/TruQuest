using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("closeLotteryInFailure")]
public class CloseThingValidationVerifierLotteryInFailureMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
    [Parameter("uint8", "_joinedNumVerifiers", 2)]
    public int JoinedNumVerifiers { get; init; }
}
