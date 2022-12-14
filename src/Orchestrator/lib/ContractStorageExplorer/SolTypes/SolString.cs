using System.Text;

using Nethereum.Hex.HexConvertors.Extensions;

namespace ContractStorageExplorer.SolTypes;

public class SolString : SolType<string>
{
    public override int SizeBits => 32 * 8;

    public override string Value => (string)ValueObject;

    public override string HexValue => Encoding.UTF8.GetBytes(Value).ToHex(prefix: false);

    public SolString() { }

    public SolString(string value)
    {
        ValueObject = value;
    }
}