using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

using Infrastructure.Ethereum.TypedData;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("fundThingSettlementProposal")]
public class FundThingSettlementProposalMessage : FunctionMessage
{
    [Parameter("tuple", "_settlementProposal", 1, "SettlementProposalTd")]
    public SettlementProposalTd SettlementProposal { get; init; }
    [Parameter("uint8", "_v", 2)]
    public byte V { get; init; }
    [Parameter("bytes32", "_r", 3)]
    public byte[] R { get; init; }
    [Parameter("bytes32", "_s", 4)]
    public byte[] S { get; init; }
}