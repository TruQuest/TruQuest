using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("castVote")]
public class CastThingValidationPollVoteMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
    [Parameter("uint16", "_thingVerifiersArrayIndex", 2)]
    public ushort ThingVerifiersArrayIndex { get; init; }
    [Parameter("uint8", "_vote", 3)]
    public Vote Vote { get; init; }
}
