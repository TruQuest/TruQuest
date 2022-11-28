using MediatR;

using Application.Common.Interfaces;

namespace API.BackgroundServices;

public class EthereumEventTracker : BackgroundService
{
    private readonly IEthereumEventListener _ethereumEventListener;
    private readonly IPublisher _mediator;

    public EthereumEventTracker(IEthereumEventListener ethereumEventListener, IPublisher mediator)
    {
        _ethereumEventListener = ethereumEventListener;
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var @event in _ethereumEventListener.GetNext(stoppingToken))
        {
            await _mediator.Publish(@event);
        }
    }
}