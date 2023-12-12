using Nethereum.Util;
using Nethereum.Hex.HexConvertors.Extensions;

using Application.Common.Interfaces;

namespace Infrastructure.Ethereum;

internal class KeccakHasher : IKeccakHasher
{
    public byte[] Hash(params string[] values) => Sha3Keccack.Current.CalculateHashFromHex(values).HexToByteArray();
}
