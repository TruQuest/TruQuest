using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("EvidenceTD")]
public class EvidenceTD
{
    [Parameter("string", "url", 1)]
    public string URL { get; init; }
}