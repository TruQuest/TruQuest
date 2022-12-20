using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SupportingEvidenceTd")]
public class SupportingEvidenceTd
{
    [Parameter("string", "url", 1)]
    public string Url { get; init; }
}