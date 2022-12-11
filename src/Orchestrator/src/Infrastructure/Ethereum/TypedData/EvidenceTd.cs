using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("EvidenceTd")]
public class EvidenceTd
{
    [Parameter("string", "url", 1)]
    public string Url { get; init; }
}