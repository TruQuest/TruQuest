using Microsoft.Extensions.Logging;

using MediatR;

namespace Application.Ethereum.Events.ThingFunded;

public class ThingFundedEvent : INotification
{
    public string ThingIdHash { get; init; }
    public required string UserId { get; init; }
}

internal class ThingFundedEventHandler : INotificationHandler<ThingFundedEvent>
{
    private readonly ILogger<ThingFundedEventHandler> _logger;

    public ThingFundedEventHandler(ILogger<ThingFundedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ThingFundedEvent @event, CancellationToken ct)
    {
        _logger.LogInformation($"Thing {@event.ThingIdHash} funded by {@event.UserId}");
    }
}