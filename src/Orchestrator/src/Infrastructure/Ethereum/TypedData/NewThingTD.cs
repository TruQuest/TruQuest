using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("NewThingTD")]
public class NewThingTD
{
    [Parameter("string", "subjectId", 1)]
    public string SubjectId { get; init; }
    [Parameter("string", "title", 2)]
    public string Title { get; init; }
    [Parameter("string", "details", 3)]
    public string Details { get; init; }
    [Parameter("string", "imageUrl", 4)]
    public string ImageURL { get; init; }
    [Parameter("tuple[]", "evidence", 5, "EvidenceTD[]")]
    public IList<EvidenceTD> Evidence { get; init; }
    [Parameter("tuple[]", "tags", 6, "TagTD[]")]
    public IList<TagTD> Tags { get; init; }
}