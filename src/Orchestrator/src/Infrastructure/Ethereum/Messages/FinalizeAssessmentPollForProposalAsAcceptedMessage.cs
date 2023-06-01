using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("finalizePoll__Accepted")]
public class FinalizeAssessmentPollForProposalAsAcceptedMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("string", "_voteAggIpfsCid", 2)]
    public string VoteAggIpfsCid { get; init; }
    [Parameter("uint64[]", "_verifiersToSlashIndices", 3)]
    public List<ulong> VerifiersToSlashIndices { get; init; }
}