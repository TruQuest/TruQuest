using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Function("setThingValidationVerifierLotteryAddress")]
public class SetThingValidationVerifierLotteryAddressMessage : FunctionMessage
{
    [Parameter("address", "_thingValidationVerifierLotteryAddress", 1)]
    public string ThingValidationVerifierLotteryAddress { get; init; }
}
