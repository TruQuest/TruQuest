using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("UserOperationTd")]
public class UserOperationTd
{
    [Parameter("address", "sender", 1)]
    public string Sender { get; init; }
    [Parameter("uint256", "nonce", 2)]
    public BigInteger Nonce { get; init; }
    [Parameter("bytes", "initCode", 3)]
    public byte[] InitCode { get; init; }
    [Parameter("bytes", "callData", 4)]
    public byte[] CallData { get; init; }
    [Parameter("uint256", "callGasLimit", 5)]
    public BigInteger CallGasLimit { get; init; }
    [Parameter("uint256", "verificationGasLimit", 6)]
    public BigInteger VerificationGasLimit { get; init; }
    [Parameter("uint256", "preVerificationGas", 7)]
    public BigInteger PreVerificationGas { get; init; }
    [Parameter("uint256", "maxFeePerGas", 8)]
    public BigInteger MaxFeePerGas { get; init; }
    [Parameter("uint256", "maxPriorityFeePerGas", 9)]
    public BigInteger MaxPriorityFeePerGas { get; init; }
    [Parameter("bytes", "paymasterAndData", 10)]
    public byte[] PaymasterAndData { get; init; }
    [Parameter("bytes", "signature", 11)]
    public byte[] Signature { get; init; }
}
