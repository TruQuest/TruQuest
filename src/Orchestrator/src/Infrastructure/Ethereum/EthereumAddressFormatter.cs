using Nethereum.Util;

using Application.Common.Interfaces;

namespace Infrastructure.Ethereum;

internal class EthereumAddressFormatter : IEthereumAddressFormatter
{
    public bool IsValidEIP55EncodedAddress(string address)
    {
        var addressUtil = AddressUtil.Current;
        return addressUtil.IsValidEthereumAddressHexFormat(address) &&
            addressUtil.IsChecksumAddress(address);
    }
}