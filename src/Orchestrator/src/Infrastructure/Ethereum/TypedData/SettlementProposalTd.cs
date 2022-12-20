using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SettlementProposalTd")]
public class SettlementProposalTd
{
    [Parameter("string", "thingId", 1)]
    public string ThingId { get; init; }
    [Parameter("string", "id", 2)]
    public string Id { get; init; }
}