using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Function("setRestrictedAccess")]
public class SetRestrictedAccessMessage : FunctionMessage
{
    [Parameter("address", "_restrictedAccessAddress", 1)]
    public string RestrictedAccessAddress { get; init; }
}
