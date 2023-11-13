using GoThataway;

namespace Application.Common.Interfaces;

public interface IContractEventListener
{
    IAsyncEnumerable<IEvent> GetNext(CancellationToken stoppingToken);
}
