using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("closeLotteryWithSuccess")]
public class CloseVerifierLotteryWithSuccessMessage : FunctionMessage
{
    [Parameter("string", "_thingId", 1)]
    public string ThingId { get; init; }
    [Parameter("bytes32", "_data", 2)]
    public byte[] Data { get; init; }
    [Parameter("uint64[]", "_winnerIndices", 3)]
    public IList<ulong> WinnerIndices { get; init; }
}