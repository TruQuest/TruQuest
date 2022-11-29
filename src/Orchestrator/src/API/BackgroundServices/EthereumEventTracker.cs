using MediatR;

using Application.Common.Interfaces;

namespace API.BackgroundServices;

public class EthereumEventTracker : BackgroundService
{
    private readonly IEthereumEventListener _ethereumEventListener;
    private readonly IServiceProvider _serviceProvider;

    public EthereumEventTracker(
        IEthereumEventListener ethereumEventListener,
        IServiceProvider serviceProvider
    )
    {
        _ethereumEventListener = ethereumEventListener;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var @event in _ethereumEventListener.GetNext(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();
            await mediator.Publish(@event);
        }
    }
}