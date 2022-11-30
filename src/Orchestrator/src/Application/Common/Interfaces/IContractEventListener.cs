using MediatR;

namespace Application.Common.Interfaces;

public interface IContractEventListener
{
    IAsyncEnumerable<INotification> GetNext(CancellationToken stoppingToken);
}