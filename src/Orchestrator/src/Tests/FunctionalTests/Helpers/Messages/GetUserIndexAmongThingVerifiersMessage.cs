using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("getUserIndexAmongThingVerifiers", "int256")]
public class GetUserIndexAmongThingVerifiersMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
    [Parameter("address", "_user", 2)]
    public string User { get; init; }
}