using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignUpTd")]
internal class SignUpTd
{
    [Parameter("string", "username", 1)]
    public string Username { get; init; }
}