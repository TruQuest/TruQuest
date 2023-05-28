using System.Diagnostics;
using System.Numerics;

using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolUint256 : SolValueType<BigInteger>, ISolNumber
{
    public override int SizeBits => 256;

    public override BigInteger Value => (BigInteger)ValueObject;

    public override string HexValue
    {
        get
        {
            var valueBytes = Value.ToByteArray(isUnsigned: IsUnsigned, isBigEndian: true);
            var sizeBytes = SizeBits / 8;
            if (valueBytes.Length < sizeBytes)
            {
                valueBytes = Enumerable.Repeat<byte>(0, sizeBytes - valueBytes.Length).Concat(valueBytes).ToArray();
            }

            Debug.Assert(valueBytes.Length == SizeBits / 8);

            return valueBytes.ToHex(prefix: false);
        }
    }

    public bool IsUnsigned => true;

    public SolUint256() { }

    public SolUint256(BigInteger value)
    {
        ValueObject = value;
    }
}