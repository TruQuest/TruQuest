namespace Application.Common.Interfaces;

public interface IEthereumAddressFormatter
{
    bool IsValidEIP55EncodedAddress(string address);
}