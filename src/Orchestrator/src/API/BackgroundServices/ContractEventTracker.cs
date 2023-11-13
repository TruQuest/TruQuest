using GoThataway;

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
            // @@TODO!!: Retry-or-archive mechanism.
            var thataway = scope.ServiceProvider.GetRequiredService<Thataway>();
            await thataway.Dispatch(@event);
        }
    }
}
