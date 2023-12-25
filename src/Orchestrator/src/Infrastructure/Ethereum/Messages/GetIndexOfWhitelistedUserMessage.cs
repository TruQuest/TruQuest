using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("getIndexOfWhitelistedUser", "int16")]
public class GetIndexOfWhitelistedUserMessage : FunctionMessage
{
    [Parameter("address", "_user", 1)]
    public string User { get; init; }
}
