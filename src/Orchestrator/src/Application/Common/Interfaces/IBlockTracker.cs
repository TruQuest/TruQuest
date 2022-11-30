namespace Application.Common.Interfaces;

public interface IBlockTracker
{
    IAsyncEnumerable<long> GetNext(CancellationToken stoppingToken);
}