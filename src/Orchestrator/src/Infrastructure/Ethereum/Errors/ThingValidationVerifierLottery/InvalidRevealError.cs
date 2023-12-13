using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Errors.ThingValidationVerifierLottery;

[Error("ThingValidationVerifierLottery__InvalidReveal")]
public class InvalidRevealError : BaseError
{
    [Parameter("bytes16", "thingId", 1)]
    public byte[] ThingId { get; set; }
}
