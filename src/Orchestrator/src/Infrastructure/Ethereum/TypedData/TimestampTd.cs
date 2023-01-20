using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("TimestampTd")]
internal class TimestampTd
{
    [Parameter("string", "timestamp", 1)]
    public string Timestamp { get; init; }
}