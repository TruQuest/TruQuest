using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("ThingTD")]
public class ThingTD
{
    [Parameter("string", "id", 1)]
    public string Id { get; init; }
}