using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolBytes12 : SolValueType<byte[]>, ISolBytesX
{
    public override int SizeBits => 12 * 8;

    protected internal override object ValueObject
    {
        get => base.ValueObject;
        set
        {
            if (value == null)
            {
                throw new ArgumentException("Can't be null");
            }
            if (value is byte[] bytes)
            {
                if (bytes.Length > 12)
                {
                    throw new ArgumentException("Too long");
                }
                else if (bytes.Length < 12)
                {
                    value = Enumerable.Repeat<byte>(0, 12 - bytes.Length).Concat(bytes).ToArray();
                }
            }
            else
            {
                throw new ArgumentException("Invalid type");
            }

            base.ValueObject = value;
        }
    }

    public override byte[] Value => (byte[])ValueObject;

    public override string HexValue => Value.ToHex(prefix: false);

    public SolBytes12() { }

    public SolBytes12(byte[] value)
    {
        ValueObject = value;
    }
}