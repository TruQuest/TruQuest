using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("giveAccessTo")]
public class GiveAccessToMessage : FunctionMessage
{
    [Parameter("address", "_user", 1)]
    public string User { get; init; }
}
