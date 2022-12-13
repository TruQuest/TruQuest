using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;

namespace ContractStorageExplorer;

public static class HexBigIntegerExtension
{
    public static string ToPaddedHexValue(this HexBigInteger value) => value.HexValue.RemoveHexPrefix().PadLeft(64, '0');
}