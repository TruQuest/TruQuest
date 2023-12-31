using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("initLottery")]
public class InitThingValidationVerifierLotteryMessage : FunctionMessage
{
    [Parameter("bytes16", "_thingId", 1)]
    public byte[] ThingId { get; init; }
    [Parameter("bytes32", "_dataHash", 2)]
    public byte[] DataHash { get; init; }
    [Parameter("bytes32", "_userXorDataHash", 3)]
    public byte[] UserXorDataHash { get; init; }
}
