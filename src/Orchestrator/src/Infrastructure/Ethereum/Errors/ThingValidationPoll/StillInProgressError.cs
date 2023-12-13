using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Errors.ThingValidationPoll;

[Error("ThingValidationPoll__StillInProgress")]
public class StillInProgressError : BaseError
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; set; }
}
