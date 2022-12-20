using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolBytes32 : SolValueType<byte[]>, ISolBytesX
{
    public override int SizeBits => 32 * 8;

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
                if (bytes.Length > 32)
                {
                    throw new ArgumentException("Too long");
                }
                else if (bytes.Length < 32)
                {
                    value = Enumerable.Repeat<byte>(0, 32 - bytes.Length).Concat(bytes).ToArray();
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

    public SolBytes32() { }

    public SolBytes32(byte[] value)
    {
        ValueObject = value;
    }
}