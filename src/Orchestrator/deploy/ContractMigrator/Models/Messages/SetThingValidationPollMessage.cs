using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Function("setThingValidationPoll")]
public class SetThingValidationPollMessage : FunctionMessage
{
    [Parameter("address", "_thingValidationPollAddress", 1)]
    public string ThingValidationPollAddress { get; init; }
}
