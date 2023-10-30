using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("finalizePoll__Unsettled")]
internal class FinalizeSettlementProposalAssessmentPollAsUnsettledMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("string", "_voteAggIpfsCid", 2)]
    public string VoteAggIpfsCid { get; init; }
    [Parameter("uint8", "_decision", 3)]
    public Decision Decision { get; init; }
    [Parameter("uint64[]", "_verifiersToSlashIndices", 4)]
    public List<ulong> VerifiersToSlashIndices { get; init; }
}
