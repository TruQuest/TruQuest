using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("ThingTd")]
public class ThingTd
{
    [Parameter("string", "id", 1)]
    public string Id { get; init; }
}