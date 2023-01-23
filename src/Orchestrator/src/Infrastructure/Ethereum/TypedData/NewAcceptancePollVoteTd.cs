using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("NewAcceptancePollVoteTd")]
public class NewAcceptancePollVoteTd
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; init; }
    [Parameter("string", "castedAt", 2)]
    public string CastedAt { get; init; }
    [Parameter("string", "decision", 3)]
    public string Decision { get; init; }
    [Parameter("string", "reason", 4)]
    public string Reason { get; init; }
}