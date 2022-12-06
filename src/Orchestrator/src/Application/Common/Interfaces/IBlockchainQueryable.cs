namespace Application.Common.Interfaces;

public interface IBlockchainQueryable
{
    Task<long> GetBlockTimestamp(long blockNumber);
}