using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("TagTd")]
public class TagTd
{
    [Parameter("int32", "id", 1)]
    public int Id { get; init; }
}