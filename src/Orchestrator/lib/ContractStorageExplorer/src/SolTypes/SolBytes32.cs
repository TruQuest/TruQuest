using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolBytes32 : SolValueType<byte[]>
{
    public override int SizeBits => 32 * 8;

    protected internal override object ValueObject
    {
        get => base.ValueObject;
        set
        {
            if (value == null || ((byte[])value).Length != 32)
            {
                throw new ArgumentException("Must be 32 bytes long");
            }
            base.ValueObject = value;
        }
    }

    public override byte[] Value => (byte[])ValueObject;

    public override string HexValue => Value.ToHex(prefix: false);

    public SolBytes32() { }

    public SolBytes32(byte[] value)
    {
        ValueObject = value;
    }
}