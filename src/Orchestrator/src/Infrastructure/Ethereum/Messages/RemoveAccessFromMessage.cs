using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("removeAccessFrom")]
public class RemoveAccessFromMessage : FunctionMessage
{
    [Parameter("address", "_user", 1)]
    public string User { get; init; }
    [Parameter("uint16", "_whitelistIndex", 2)]
    public ushort WhitelistIndex { get; init; }
}
