using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolAddress : SolValueType<string>
{
    public override int SizeBits => 20 * 8;

    protected internal override object ValueObject
    {
        get => base.ValueObject;
        set
        {
            base.ValueObject = _checkValue((string)value);
        }
    }

    public override string Value => (string)ValueObject;

    public override string HexValue => Value;

    public SolAddress() { }

    public SolAddress(string value)
    {
        ValueObject = value;
    }

    private string _checkValue(string value)
    {
        value = value.RemoveHexPrefix();
        if (value.Length < 40)
        {
            throw new ArgumentException("Invalid address");
        }
        else if (value.Length > 40)
        {
            value = string.Join(string.Empty, value.TakeLast(40));
        }

        return value;
    }
}