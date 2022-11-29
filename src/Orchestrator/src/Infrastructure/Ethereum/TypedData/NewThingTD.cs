using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("NewThingTd")]
public class NewThingTd
{
    [Parameter("string", "subjectId", 1)]
    public string SubjectId { get; init; }
    [Parameter("string", "title", 2)]
    public string Title { get; init; }
    [Parameter("string", "details", 3)]
    public string Details { get; init; }
    [Parameter("string", "imageUrl", 4)]
    public string ImageUrl { get; init; }
    [Parameter("tuple[]", "evidence", 5, "EvidenceTd[]")]
    public IList<EvidenceTd> Evidence { get; init; }
    [Parameter("tuple[]", "tags", 6, "TagTd[]")]
    public IList<TagTd> Tags { get; init; }
}