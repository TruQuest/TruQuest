using System.Numerics;

using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolInt8 : SolValueType<sbyte>, ISolNumber
{
    public override int SizeBits => 8;

    public override sbyte Value => (sbyte)(BigInteger)ValueObject;

    public override string HexValue
    {
        get
        {
            var valueBytes = ((BigInteger)ValueObject).ToByteArray(isUnsigned: IsUnsigned, isBigEndian: true);
            return valueBytes.ToHex(prefix: false);
        }
    }

    public bool IsUnsigned => false;

    public SolInt8() { }

    public SolInt8(sbyte value)
    {
        ValueObject = new BigInteger(value);
    }
}