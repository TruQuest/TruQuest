using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolBytes32 : SolValueType
{
    public static int SizeBits => 32 * 8;

    public byte[] Value { get; }

    public SolBytes32(byte[] value)
    {
        if (value.Length != 32)
        {
            throw new ArgumentException("Must be 32 bytes long");
        }
        Value = value;
    }

    public override string HexValue => Value.ToHex(prefix: false);
}