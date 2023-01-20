using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignInTd")]
internal class SignInTd
{
    [Parameter("string", "timestamp", 1)]
    public string Timestamp { get; init; }
    [Parameter("string", "orchestratorSignature", 2)]
    public string OrchestratorSignature { get; init; }
}