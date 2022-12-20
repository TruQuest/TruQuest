using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("NewSettlementProposalTd")]
public class NewSettlementProposalTd
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; init; }
    [Parameter("string", "title", 2)]
    public string Title { get; init; }
    [Parameter("int8", "verdict", 3)]
    public int Verdict { get; init; }
    [Parameter("string", "details", 4)]
    public string Details { get; init; }
    [Parameter("tuple[]", "evidence", 5, "SupportingEvidenceTd[]")]
    public List<SupportingEvidenceTd> Evidence { get; init; }
}