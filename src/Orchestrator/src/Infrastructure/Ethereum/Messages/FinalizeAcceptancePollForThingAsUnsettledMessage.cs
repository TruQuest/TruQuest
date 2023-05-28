using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("finalizePoll__Unsettled")]
internal class FinalizeAcceptancePollForThingAsUnsettledMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
    [Parameter("string", "_voteAggIpfsCid", 2)]
    public string VoteAggIpfsCid { get; init; }
    [Parameter("uint8", "_decision", 3)]
    public Decision Decision { get; init; }
    [Parameter("uint64[]", "_verifiersToSlashIndices", 4)]
    public List<ulong> VerifiersToSlashIndices { get; init; }
}