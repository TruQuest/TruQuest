using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("TagTD")]
public class TagTD
{
    [Parameter("int32", "id", 1)]
    public int Id { get; init; }
}