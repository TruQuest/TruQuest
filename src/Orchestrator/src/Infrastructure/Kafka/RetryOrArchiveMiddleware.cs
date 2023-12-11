using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using KafkaFlow;
using GoThataway;

using Domain.Aggregates;
using Application.Common.Monitoring;
using Application.General.Commands.ArchiveDeadLetter;

namespace Infrastructure.Kafka;

internal class RetryOrArchiveMiddleware : IMessageMiddleware
{
    private readonly ILogger<RetryOrArchiveMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RetryOrArchiveMiddleware(
        ILogger<RetryOrArchiveMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        int maxAttempts = 3; // @@TODO: Config.
        do
        {
            await next(context);

            byte[]? handleError = null;
            if ((handleError = context.Headers["HandleError"]) != null)
            {
                if (handleError[0] == 1) // retryable error
                {
                    var attemptsMade = (context.Headers["AttemptsMade"] ?? new byte[1] { 0 })[0] + 1;
                    if (attemptsMade < maxAttempts)
                    {
                        context.Headers["HandleError"] = null;
                        context.Headers["AttemptsMade"] = new byte[1] { (byte)attemptsMade };

                        _logger.LogWarning("A retryable error occured. Retrying...");

                        await Task.Delay(500); // @@TODO: Config.
                        continue;
                    }
                }

                _logger.LogError("An unretryable (or a retryable with max attempts exhausted) error occured. Archiving...");

                using var scope = _serviceScopeFactory.CreateScope();
                var thataway = scope.ServiceProvider.GetRequiredService<Thataway>();

                var deadLetter = new DeadLetter(
                    source: DeadLetterSource.ActionableEventFromKafka,
                    archivedAt: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                );
                var payload = new Dictionary<string, object>()
                {
                    ["Key"] = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                    ["Headers"] = new Dictionary<string, string>(
                        ((IEnumerable<KeyValuePair<string, byte[]>>)context.Headers)
                            .Where(h => h.Key != "traceparent" && h.Key != "HandleError" && h.Key != "AttemptsMade")
                            .Select(h => KeyValuePair.Create(h.Key, Encoding.UTF8.GetString(h.Value)))
                    ),
                    ["Body"] = context.Message.Value
                };

                Telemetry.CurrentActivity!.AddTraceparentTo(payload);
                deadLetter.SetPayload(payload);

                await thataway.Send(new ArchiveDeadLetterCommand
                {
                    DeadLetter = deadLetter
                });
            }

            return;
        } while (true);
    }
}
