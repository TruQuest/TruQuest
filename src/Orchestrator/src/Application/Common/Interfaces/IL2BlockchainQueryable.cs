using System.Numerics;

namespace Application.Common.Interfaces;

public interface IL2BlockchainQueryable
{
    Task<bool> CheckContractDeployed(string address);
    Task<BigInteger> GetBaseFee();
    Task<BigInteger> GetMaxPriorityFee();
}
