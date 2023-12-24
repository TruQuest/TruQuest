using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("stopTheWorld")]
public class StopTheWorldMessage : FunctionMessage
{
    [Parameter("bool", "_value", 1)]
    public bool Value { get; init; }
}
