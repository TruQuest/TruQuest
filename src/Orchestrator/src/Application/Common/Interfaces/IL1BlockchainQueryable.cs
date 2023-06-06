namespace Application.Common.Interfaces;

public interface IL1BlockchainQueryable
{
    Task<byte[]> GetBlockHash(long blockNumber);
    Task<long> GetBlockTimestamp(long blockNumber);
}