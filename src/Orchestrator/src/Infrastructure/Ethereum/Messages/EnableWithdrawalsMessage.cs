using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("enableWithdrawals")]
public class EnableWithdrawalsMessage : FunctionMessage
{
    [Parameter("bool", "_value", 1)]
    public bool Value { get; init; }
}
