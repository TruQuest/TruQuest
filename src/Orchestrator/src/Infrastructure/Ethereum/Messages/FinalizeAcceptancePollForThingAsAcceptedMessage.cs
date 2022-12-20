using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("finalizePoll__Accepted")]
public class FinalizeAcceptancePollForThingAsAcceptedMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
    [Parameter("string", "_voteAggIpfsCid", 2)]
    public string VoteAggIpfsCid { get; init; }
    [Parameter("address[]", "_verifiersToReward", 3)]
    public List<string> VerifiersToReward { get; init; }
    [Parameter("address[]", "_verifiersToSlash", 4)]
    public List<string> VerifiersToSlash { get; init; }
}