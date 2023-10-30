using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("finalizePoll__Accepted")]
public class FinalizeThingValidationPollAsAcceptedMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
    [Parameter("string", "_voteAggIpfsCid", 2)]
    public string VoteAggIpfsCid { get; init; }
    [Parameter("uint64[]", "_verifiersToSlashIndices", 3)]
    public List<ulong> VerifiersToSlashIndices { get; init; }
}
