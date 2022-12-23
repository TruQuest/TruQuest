using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("castVote")]
public class CastVoteMessage : FunctionMessage
{
    [Parameter("bytes32", "_combinedId", 1)]
    public byte[] CombinedId { get; init; }
    [Parameter("uint8", "_vote", 2)]
    public Vote Vote { get; init; }
}