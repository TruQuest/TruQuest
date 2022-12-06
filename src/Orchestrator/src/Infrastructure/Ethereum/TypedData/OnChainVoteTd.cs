using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("OnChainVoteTd")]
public class OnChainVoteTd
{
    [Parameter("int64", "blockNumber", 1)]
    public long BlockNumber { get; init; }
    [Parameter("int32", "txnIndex", 2)]
    public int TxnIndex { get; init; }
    [Parameter("string", "thingIdHash", 3)]
    public string ThingIdHash { get; init; }
    [Parameter("address", "userId", 4)]
    public string UserId { get; init; }
    [Parameter("string", "decision", 5)]
    public string Decision { get; init; }
}