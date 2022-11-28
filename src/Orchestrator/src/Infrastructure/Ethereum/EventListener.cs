using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using MediatR;
using Nethereum.Web3;

using Application.Common.Interfaces;
using AppEvents = Application.Ethereum.Events;

using Infrastructure.Ethereum.Events;

namespace Infrastructure.Ethereum;

internal class EventListener : IEthereumEventListener
{
    private readonly ILogger<EventListener> _logger;
    private readonly string _url;
    private readonly string _truQuestAddress;

    public EventListener(IConfiguration configuration, ILogger<EventListener> logger)
    {
        _logger = logger;
        var network = configuration["Ethereum:Network"]!;
        _url = configuration[$"Ethereum:Networks:{network}:URL"]!;
        _truQuestAddress = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"]!;
    }

    public async IAsyncEnumerable<INotification> GetNext([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        var web3 = new Web3(_url);
        var thingFundedEventCtx = web3.Eth.GetEvent<ThingFundedEvent>();
        var filterInput = thingFundedEventCtx.CreateFilterInput();
        var filterId = await thingFundedEventCtx.CreateFilterAsync(filterInput);

        while (!stoppingToken.IsCancellationRequested)
        {
            var thingFundedEvents = await thingFundedEventCtx.GetFilterChangesAsync(filterId);
            foreach (var @event in thingFundedEvents)
            {
                yield return new AppEvents.ThingFunded.ThingFundedEvent
                {
                    ThingIdHash = @event.Event.ThingIdHash,
                    UserId = @event.Event.UserId
                };
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}