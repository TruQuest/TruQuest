using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolBytes16 : SolValueType<byte[]>, ISolBytesX
{
    public override int SizeBits => 16 * 8;

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
                if (bytes.Length > 16)
                {
                    throw new ArgumentException("Too long");
                }
                else if (bytes.Length < 16)
                {
                    value = Enumerable.Repeat<byte>(0, 16 - bytes.Length).Concat(bytes).ToArray();
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

    public SolBytes16() { }

    public SolBytes16(byte[] value)
    {
        ValueObject = value;
    }
}