using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("giveAccessToMany")]
public class GiveAccessToManyMessage : FunctionMessage
{
    [Parameter("address[]", "_users", 1)]
    public List<string> Users { get; set; }
}
