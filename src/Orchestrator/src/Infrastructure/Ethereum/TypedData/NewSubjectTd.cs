using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("NewSubjectTd")]
public class NewSubjectTd
{
    [Parameter("int8", "type", 1)]
    public int Type { get; init; }

    [Parameter("string", "name", 2)]
    public string Name { get; init; }

    [Parameter("string", "details", 3)]
    public string Details { get; init; }

    [Parameter("string", "imageUrl", 4)]
    public string ImageUrl { get; init; }

    [Parameter("tuple[]", "tags", 5, "TagTd[]")]
    public List<TagTd> Tags { get; init; }
}