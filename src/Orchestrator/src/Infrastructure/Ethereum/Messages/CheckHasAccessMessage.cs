using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("checkHasAccess", "bool")]
public class CheckHasAccessMessage : FunctionMessage
{
    [Parameter("address", "_user", 1)]
    public string User { get; init; }
}
