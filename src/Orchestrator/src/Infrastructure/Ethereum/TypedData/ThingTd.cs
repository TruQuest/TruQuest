using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("ThingTd")]
public class ThingTd
{
    [Parameter("bytes16", "id", 1)]
    public byte[] Id { get; init; }
}