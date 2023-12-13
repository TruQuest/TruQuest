using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Function("enableWithdrawals")]
public class EnableWithdrawalsMessage : FunctionMessage
{
    [Parameter("bool", "_value", 1)]
    public bool Value { get; init; }
}
