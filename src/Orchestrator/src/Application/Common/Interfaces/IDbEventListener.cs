namespace Application.Common.Interfaces;

public interface IDbEventListener : IDisposable
{
    IAsyncEnumerable<string> GetNext(CancellationToken stoppingToken);
}