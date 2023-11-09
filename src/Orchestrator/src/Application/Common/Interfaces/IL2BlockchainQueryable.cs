using System.Numerics;

namespace Application.Common.Interfaces;

public interface IL2BlockchainQueryable
{
    Task<bool> CheckContractDeployed(string address);
    Task<BigInteger> GetBaseFee();
    Task<BigInteger> GetMaxPriorityFee();
    Task<long> GetCorrespondingL1BlockNumberFor(long l2Block);
    Task<(BigInteger InWei, double InEther)> GetBalance(string address);
}
