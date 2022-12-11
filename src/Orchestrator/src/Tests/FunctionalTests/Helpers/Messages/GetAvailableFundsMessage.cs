using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("getAvailableFunds", "uint256")]
public class GetAvailableFundsMessage : FunctionMessage
{
    [Parameter("address", "_user", 1)]
    public string User { get; init; }
}