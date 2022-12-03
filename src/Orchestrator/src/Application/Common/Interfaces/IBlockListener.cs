namespace Application.Common.Interfaces;

public interface IBlockListener
{
    IAsyncEnumerable<long> GetNext(CancellationToken stoppingToken);
}