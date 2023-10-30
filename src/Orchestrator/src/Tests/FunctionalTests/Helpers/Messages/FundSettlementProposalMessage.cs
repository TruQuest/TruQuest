using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("fundSettlementProposal")]
public class FundSettlementProposalMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
    [Parameter("bytes16", "_proposalId", 2)]
    public byte[] ProposalId { get; init; }
    [Parameter("uint8", "_v", 3)]
    public byte V { get; init; }
    [Parameter("bytes32", "_r", 4)]
    public byte[] R { get; init; }
    [Parameter("bytes32", "_s", 5)]
    public byte[] S { get; init; }
}
