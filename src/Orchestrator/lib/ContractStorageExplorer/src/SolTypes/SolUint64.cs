using System.Collections;
using System.Diagnostics;
using System.Numerics;

using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolUint64 : SolValueType<ulong>, ISolNumber
{
    public override int SizeBits => 64;

    public override ulong Value => (ulong)(BigInteger)ValueObject;

    public override string HexValue
    {
        get
        {
            var valueBytes = ((BigInteger)ValueObject).ToByteArray(isUnsigned: IsUnsigned, isBigEndian: true);
            var sizeBytes = SizeBits / 8;
            if (valueBytes.Length < sizeBytes)
            {
                var value = Value;
                if (value >= 0)
                {
                    valueBytes = Enumerable.Repeat<byte>(0, sizeBytes - valueBytes.Length).Concat(valueBytes).ToArray();
                }
                else
                {
                    var valueBits = new BitArray(valueBytes);
                    valueBits = valueBits.TwosComplement();
                    valueBytes = valueBits.ToByteArray();
                    valueBytes = Enumerable.Repeat<byte>(0, sizeBytes - valueBytes.Length).Concat(valueBytes).ToArray();

                    valueBits = new BitArray(valueBytes);
                    valueBits = valueBits.TwosComplement();
                    valueBytes = valueBits.ToByteArray();
                }
            }

            Debug.Assert(valueBytes.Length == SizeBits / 8);

            return valueBytes.ToHex(prefix: false);
        }
    }

    public bool IsUnsigned => true;

    public SolUint64() { }

    public SolUint64(ulong value)
    {
        ValueObject = new BigInteger(value);
    }
}