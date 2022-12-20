using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SettlementProposalTd")]
public class SettlementProposalTd
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; init; }
    [Parameter("bytes16", "id", 2)]
    public byte[] Id { get; init; }
}