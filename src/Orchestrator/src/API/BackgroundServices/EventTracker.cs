using System.Reflection;
using System.Transactions;

using MediatR;

using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Thing.Events.ThingFunded;

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
            using var thingFundedNotificationListener = _serviceProvider.GetRequiredService<IThingFundedNotificationListener>();
            await foreach (var notification in thingFundedNotificationListener.GetNext(stoppingToken))
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();
                var notificationSplit = notification.Split("::");
                var eventTypeName = notificationSplit.First();

                if (eventTypeName == typeof(ThingFundedEvent).Name)
                {
                    var attr = typeof(ThingFundedEvent).GetCustomAttribute<ExecuteInTxnAttribute>()!;
                    var sharedTxnScope = scope.ServiceProvider.GetRequiredService<ISharedTxnScope>();
                    sharedTxnScope.Init(attr.ExcludeRepos);

                    using var txnScope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions { IsolationLevel = attr.IsolationLevel },
                        TransactionScopeAsyncFlowOption.Enabled
                    );

                    await mediator.Publish(new ThingFundedEvent
                    {
                        Id = long.Parse(notificationSplit.Skip(1).First()),
                        ThingIdHash = notificationSplit.Last()
                    });

                    txnScope.Complete();
                }
            }
        }
    }
}