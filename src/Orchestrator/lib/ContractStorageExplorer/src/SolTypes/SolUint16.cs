using System.Numerics;

using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolUint16 : SolValueType<ushort>, ISolNumber
{
    public override int SizeBits => 16;

    public override ushort Value => (ushort)(BigInteger)ValueObject;

    public override string HexValue
    {
        get
        {
            var valueBytes = ((BigInteger)ValueObject).ToByteArray(isUnsigned: IsUnsigned, isBigEndian: true);
            return valueBytes.ToHex(prefix: false);
        }
    }

    public bool IsUnsigned => true;

    public SolUint16() { }

    public SolUint16(ushort value)
    {
        ValueObject = new BigInteger(value);
    }
}