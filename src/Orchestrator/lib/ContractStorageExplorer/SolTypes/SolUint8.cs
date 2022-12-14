using System.Numerics;

using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolUint8 : SolValueType<byte>, ISolNumber
{
    public override int SizeBits => 8;

    public override byte Value => (byte)(BigInteger)ValueObject;

    public override string HexValue
    {
        get
        {
            var valueBytes = ((BigInteger)ValueObject).ToByteArray(isUnsigned: IsUnsigned, isBigEndian: true);
            return valueBytes.ToHex(prefix: false);
        }
    }

    public bool IsUnsigned => true;

    public SolUint8() { }

    public SolUint8(byte value)
    {
        ValueObject = new BigInteger(value);
    }
}