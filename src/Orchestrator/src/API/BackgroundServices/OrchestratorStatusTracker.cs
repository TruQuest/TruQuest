using System.Numerics;

using Application;
using Application.Common.Interfaces;
using Infrastructure.Ethereum;

namespace API.BackgroundServices;

public class OrchestratorStatusTracker : BackgroundService
{
    private readonly IL2BlockchainQueryable _l2BlockchainQueryable;
    private readonly IServiceProvider _serviceProvider;

    private readonly string _orchestratorAddress;

    public OrchestratorStatusTracker(
        IL2BlockchainQueryable l2BlockchainQueryable,
        IServiceProvider serviceProvider,
        AccountProvider accountProvider
    )
    {
        _l2BlockchainQueryable = l2BlockchainQueryable;
        _serviceProvider = serviceProvider;
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

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30)); // @@TODO: Config.

            var (inWei, inEther) = await _l2BlockchainQueryable.GetBalance(_orchestratorAddress);
            lock (this)
            {
                balanceInWei = inWei;
                balanceInEther = inEther;
            }
        }
    }
}
