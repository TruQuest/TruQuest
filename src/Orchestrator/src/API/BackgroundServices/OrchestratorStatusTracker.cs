using System.Numerics;

using Application;
using Application.Common.Interfaces;
using Infrastructure.Ethereum;

namespace API.BackgroundServices;

public class OrchestratorStatusTracker : BackgroundService
{
    private readonly IL2BlockchainQueryable _l2BlockchainQueryable;
    private readonly IEmailForwarder _emailForwarder;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly string _orchestratorAddress;

    public OrchestratorStatusTracker(
        IL2BlockchainQueryable l2BlockchainQueryable,
        IEmailForwarder emailForwarder,
        IServiceScopeFactory serviceScopeFactory,
        IAccountProvider accountProvider
    )
    {
        _l2BlockchainQueryable = l2BlockchainQueryable;
        _emailForwarder = emailForwarder;
        _serviceScopeFactory = serviceScopeFactory;
        _orchestratorAddress = accountProvider.GetAccount("Orchestrator").Address;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        BigInteger balanceInWei = 0;
        double balanceInEther = 0;
        Metrics.OnOrchestratorBalanceObserved = () =>
        {
            lock (this)
            {
                return balanceInEther;
            }
        };

        int deadLetterCount = 0;
        Metrics.OnDeadLetterCountObserved = () =>
        {
            lock (this) // Should use different locks for different metrics, but whatever...
            {
                return deadLetterCount;
            }
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30)); // @@TODO: Config.

            var (inWei, inEther) = await _l2BlockchainQueryable.GetBalance(_orchestratorAddress);
            lock (this)
            {
                balanceInWei = inWei;
                balanceInEther = inEther;
            }

            using var scope = _serviceScopeFactory.CreateScope();

            var deadLetterQueryable = scope.ServiceProvider.GetRequiredService<IDeadLetterQueryable>();
            int count = await deadLetterQueryable.GetUnhandledCount();
            lock (this)
            {
                deadLetterCount = count;
            }

            await _emailForwarder.FetchAndForward();
        }
    }
}
