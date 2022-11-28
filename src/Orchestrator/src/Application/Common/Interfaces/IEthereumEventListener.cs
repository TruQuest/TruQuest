using MediatR;

namespace Application.Common.Interfaces;

public interface IEthereumEventListener
{
    IAsyncEnumerable<INotification> GetNext(CancellationToken stoppingToken);
}