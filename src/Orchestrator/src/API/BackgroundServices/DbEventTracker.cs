using System.Reflection;
using System.Transactions;

using MediatR;

using Domain.Aggregates;
using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Thing.Events.ThingFunded;
using Application.Thing.Commands.InitVerifierLottery;

namespace API.BackgroundServices;

public class DbEventTracker : BackgroundService
{
    private readonly ILogger<DbEventTracker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DbEventTracker(ILogger<DbEventTracker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var dbEventListener = _serviceProvider.GetRequiredService<IDbEventListener>();
            await foreach (var @event in dbEventListener.GetNext(stoppingToken))
            {
                using var scope = _serviceProvider.CreateScope();
                var eventSplit = @event.Split("::");
                var typeName = eventSplit.First();

                if (typeName == typeof(ThingFundedEvent).Name)
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

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
                        Id = long.Parse(eventSplit.Skip(1).First()),
                        BlockNumber = long.Parse(eventSplit.Skip(2).First()),
                        ThingIdHash = eventSplit.Last()
                    });

                    txnScope.Complete();
                }
                else if (typeName == typeof(Thing).Name)
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                    var thingId = Guid.Parse(eventSplit.Skip(1).First());
                    var state = (ThingState)int.Parse(eventSplit.Last());
                    switch (state)
                    {
                        case ThingState.Funded:
                            await mediator.Send(new InitVerifierLotteryCommand
                            {
                                ThingId = thingId
                            });
                            break;
                        case ThingState.VerifierLotteryInProgress:
                            _logger.LogInformation("Lottery in progress");
                            break;
                    }
                }
            }
        }
    }
}