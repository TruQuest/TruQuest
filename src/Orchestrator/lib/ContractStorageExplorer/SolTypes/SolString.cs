using System.Text;

using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolString : SolType
{
    public static int SizeBits => 32 * 8;

    public string Value { get; }

    public SolString(string value)
    {
        Value = value;
    }

    public override string HexValue => Encoding.UTF8.GetBytes(Value).ToHex(prefix: false);
}