using Microsoft.Extensions.Logging;

using KafkaFlow;

namespace Infrastructure.Kafka;

internal class RetryOrArchiveMiddleware : IMessageMiddleware
{
    private readonly ILogger<RetryOrArchiveMiddleware> _logger;

    public RetryOrArchiveMiddleware(ILogger<RetryOrArchiveMiddleware> logger)
    {
        _logger = logger;
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

                _logger.LogWarning(
                    "An unretryable (or a retryable with max attempts exhausted) error occured. Putting into Dead-letter topic"
                );

                // @@TODO!!: Put in Dead-letter topic.
            }

            return;
        } while (true);
    }
}
