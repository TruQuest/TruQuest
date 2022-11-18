using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignUpTD")]
internal class SignUpTD
{
    [Parameter("string", "username", 1)]
    public string Username { get; set; }
}