using Application.Common.Interfaces;

namespace API.BackgroundServices;

public class EventTracker : BackgroundService
{
    private readonly ILogger<EventTracker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public EventTracker(ILogger<EventTracker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var thingFundedNotificationListener = scope.ServiceProvider.GetRequiredService<IThingFundedNotificationListener>();
            await foreach (var notification in thingFundedNotificationListener.GetNext(stoppingToken))
            {
                _logger.LogInformation(notification);
            }
        }
    }
}