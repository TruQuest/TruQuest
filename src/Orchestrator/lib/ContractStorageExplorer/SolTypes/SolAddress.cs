using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolAddress : SolValueType
{
    public static int SizeBits => 20 * 8;

    public string Value { get; }

    public SolAddress(string value)
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

        Value = value;
    }

    public override string HexValue => Value;
}