using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("OnChainAcceptancePollVoteTd")]
public class OnChainAcceptancePollVoteTd
{
    [Parameter("int64", "blockNumber", 1)]
    public long BlockNumber { get; init; }
    [Parameter("int32", "txnIndex", 2)]
    public int TxnIndex { get; init; }
    [Parameter("int64", "l1BlockNumber", 3)]
    public long L1BlockNumber { get; init; }
    [Parameter("address", "userId", 4)]
    public string UserId { get; init; }
    [Parameter("string", "decision", 5)]
    public string Decision { get; init; }
    [Parameter("string", "reason", 6)]
    public string Reason { get; init; }
}