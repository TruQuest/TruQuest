using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("getParticipants", "address[]")]
public class GetThingSubmissionVerifierLotteryParticipantsMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
}
