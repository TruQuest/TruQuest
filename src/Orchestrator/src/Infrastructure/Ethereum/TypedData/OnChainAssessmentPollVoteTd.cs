using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("OnChainAssessmentPollVoteTd")]
public class OnChainAssessmentPollVoteTd
{
    [Parameter("int64", "blockNumber", 1)]
    public long BlockNumber { get; init; }
    [Parameter("int32", "txnIndex", 2)]
    public int TxnIndex { get; init; }
    [Parameter("address", "userId", 3)]
    public string UserId { get; init; }
    [Parameter("string", "decision", 4)]
    public string Decision { get; init; }
    [Parameter("string", "reason", 5)]
    public string Reason { get; init; }
}