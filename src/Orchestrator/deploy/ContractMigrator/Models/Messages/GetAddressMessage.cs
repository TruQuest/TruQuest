using System.Numerics;

using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Function("getAddress", "address")]
public class GetAddressMessage : FunctionMessage
{
    [Parameter("address", "owner", 1)]
    public string Owner { get; init; }
    [Parameter("uint256", "salt", 2)]
    public BigInteger Salt { get; init; }
}
