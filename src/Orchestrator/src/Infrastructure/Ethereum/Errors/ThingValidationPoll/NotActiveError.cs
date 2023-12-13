using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Errors.ThingValidationPoll;

[Error("ThingValidationPoll__NotActive")]
public class NotActiveError : BaseError
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; set; }
}
