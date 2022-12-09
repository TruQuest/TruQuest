using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("preJoinLottery")]
public class PreJoinLotteryMessage : FunctionMessage
{
    [Parameter("string", "_thingId", 1)]
    public string ThingId { get; init; }
    [Parameter("bytes32", "_dataHash", 2)]
    public byte[] DataHash { get; init; }
}