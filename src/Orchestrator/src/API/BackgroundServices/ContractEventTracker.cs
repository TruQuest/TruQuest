using Application;
using Application.Common.Interfaces;

namespace API.BackgroundServices;

public class ContractEventTracker : BackgroundService
{
    private readonly IContractEventListener _contractEventListener;
    private readonly IServiceProvider _serviceProvider;

    public ContractEventTracker(
        IContractEventListener contractEventListener,
        IServiceProvider serviceProvider
    )
    {
        _contractEventListener = contractEventListener;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var @event in _contractEventListener.GetNext(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<PublisherWrapper>();
            await mediator.Publish(@event);
        }
    }
}