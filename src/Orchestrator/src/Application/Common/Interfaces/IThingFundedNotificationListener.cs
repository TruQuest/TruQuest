namespace Application.Common.Interfaces;

public interface IThingFundedNotificationListener : IDisposable
{
    IAsyncEnumerable<string> GetNext(CancellationToken stoppingToken);
}