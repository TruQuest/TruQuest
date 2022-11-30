using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using MediatR;
using Nethereum.Web3;

using Application.Common.Interfaces;
using AppEvents = Application.Ethereum.Events;

using Infrastructure.Ethereum.Events;

namespace Infrastructure.Ethereum;

internal class ContractEventListener : IContractEventListener
{
    private readonly ILogger<ContractEventListener> _logger;
    private readonly string _url;
    private readonly string _truQuestAddress;

    public ContractEventListener(IConfiguration configuration, ILogger<ContractEventListener> logger)
    {
        _logger = logger;
        var network = configuration["Ethereum:Network"]!;
        _url = configuration[$"Ethereum:Networks:{network}:URL"]!;
        _truQuestAddress = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"]!;
    }

    public async IAsyncEnumerable<INotification> GetNext([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        var web3 = new Web3(_url);
        var thingFundedEventLog = web3.Eth.GetEvent<ThingFundedEvent>();
        var filterInput = thingFundedEventLog.CreateFilterInput();
        var filterId = await thingFundedEventLog.CreateFilterAsync(filterInput);
        _logger.LogInformation("Filter {filterId}", filterId.Value);

        while (!stoppingToken.IsCancellationRequested)
        {
            var thingFundedEvents = await thingFundedEventLog.GetFilterChangesAsync(filterId);
            foreach (var @event in thingFundedEvents)
            {
                yield return new AppEvents.ThingFunded.ThingFundedEvent
                {
                    BlockNumber = (long)@event.Log.BlockNumber.Value,
                    ThingIdHash = @event.Event.ThingIdHash.TrimStart('0', 'x'),
                    UserId = @event.Event.UserId.TrimStart('0', 'x'),
                    Stake = (decimal)@event.Event.Stake
                };
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}